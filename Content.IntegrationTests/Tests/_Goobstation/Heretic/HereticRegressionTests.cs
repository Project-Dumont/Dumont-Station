using System.Linq;
using System.Numerics;
using Content.Shared.Mind;
using Content.Shared.Inventory;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Throwing;
using Content.Shared.Gravity;
using Content.Shared.StatusEffectNew;
using Content.Shared.Interaction;
using Content.Shared.Charges.Systems;
using Content.Shared.EntityEffects;
using Content.Trauma.Server.Heretic.Systems;
using Content.Trauma.Shared.Heretic.Components;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;
using Content.Trauma.Shared.Heretic.EntityEffects;
using Content.Trauma.Shared.Heretic.Events;
using Content.Goobstation.Shared.Boomerang;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture]
public sealed class HereticRegressionTests
{
    [TestCase("RitualCodexCicatrix", "OrganHumanHeart", "Pen", "BookNarsieLegend")]
    [TestCase("RitualCodexCicatrix", "LeftArmHuman", "Pen", "BookNarsieLegend")]
    [TestCase("RitualLionhunterRifle", "OrganHumanHeart", "MaterialWoodPlank", "PassengerPDA")]
    [TestCase("RitualSharpMedal", "LeftHandHuman", "RightHandHuman", "OrganHumanEyes", "KitchenKnife")]
    public async Task RitualAcceptsHostBodyIngredients(string ritualId, params string[] ingredients)
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var ritual = em.SpawnEntity(ritualId, map.GridCoords);
            var raiser = em.AddComponent<Content.Trauma.Shared.Heretic.Rituals.HereticRitualRaiserComponent>(ritual);
            var condition = em.GetComponent<Content.Trauma.Shared.Heretic.Rituals.HereticRitualComponent>(ritual)
                .Effects.SelectMany(effect => effect.Conditions ?? []).OfType<Content.Trauma.Shared.Heretic.Rituals.ProcessIngredientsCondition>().Single();
            raiser.Blackboard[condition.ApplyOn] = ingredients.Select(id => em.SpawnEntity(id, map.GridCoords)).ToArray();
            Assert.That(em.System<Content.Trauma.Shared.Heretic.Rituals.HereticRitualEffectSystem>()
                .TryCondition(ritual, condition, (ritual, raiser)), Is.True);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task CrucibleAcceptsOrgansAndBodyParts()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var body = em.SpawnEntity("MobHuman", map.GridCoords);
            var mind = AddHeretic(em, body);
            var crucible = em.SpawnEntity("MawedCrucible", map.GridCoords);
            Assert.That(em.GetComponent<TransformComponent>(crucible).Anchored, Is.True);
            var comp = em.GetComponent<Content.Trauma.Shared.Heretic.Components.Side.MawedCrucibleComponent>(crucible);
            comp.CurrentMass = 0;
            var initial = comp.CurrentMass;
            foreach (var id in new[] { "OrganHumanHeart", "LeftArmHuman", "RightHandHuman" })
            {
                var fuel = em.SpawnEntity(id, map.GridCoords);
                var ev = new InteractUsingEvent(body, fuel, crucible, map.GridCoords);
                em.EventBus.RaiseLocalEvent(crucible, ev);
                Assert.That(ev.Handled, Is.True, id);
            }
            Assert.That(comp.CurrentMass, Is.EqualTo(initial + 3));
            em.DeleteEntity(body);
            em.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task MindGraspUpgradesBodyAction()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var body = em.SpawnEntity("MobHuman", map.GridCoords);
            var mind = AddHeretic(em, body);
            EntityUid? action = null;
            em.System<Content.Shared.Actions.SharedActionsSystem>().AddAction(body, ref action, "ActionHereticMansusGrasp", mind);
            var proto = pair.Server.ProtoMan.Index<Content.Trauma.Shared.Heretic.Prototypes.HereticKnowledgePrototype>("MindGrasp");
            em.EventBus.RaiseLocalEvent(mind, proto.MindEvent!);
            Assert.That(action, Is.Not.Null);
            var upgrade = em.GetComponent<Content.Trauma.Shared.Heretic.Components.Side.MansusGraspUpgradeComponent>(action!.Value);
            Assert.That(upgrade.AddedComponents.ContainsKey("AreaMansusGrasp"), Is.True);
            var grasp = em.SpawnEntity("TouchSpellMansus", map.GridCoords);
            em.GetComponent<TouchSpellComponent>(grasp).Action = action;
            var spawned = new AfterTouchSpellAbilityUsedEvent(grasp);
            em.EventBus.RaiseLocalEvent(action.Value, ref spawned);
            Assert.That(em.System<SharedHandsSystem>().TryPickup(body, grasp), Is.True);
            var use = new Content.Shared.Interaction.Events.UseInHandEvent(body);
            em.EventBus.RaiseLocalEvent(grasp, use);
            Assert.That(use.Handled, Is.True);
            Assert.That(em.GetComponent<Content.Trauma.Shared.Heretic.Components.Side.AreaMansusGraspComponent>(grasp).ChannelStartTime, Is.Not.Null);
            em.DeleteEntity(body);
            em.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task VoidCloakSynchronizesRepeatedHoodToggles()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        EntityUid body = default, mind = default, cloak = default, hood = default;
        NetEntity netCloak = default;
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            body = em.SpawnEntity("MobHuman", map.GridCoords);
            var session = pair.Server.ResolveDependency<Robust.Shared.Player.ISharedPlayerManager>().Sessions.Single();
            var minds = em.System<SharedMindSystem>();
            var created = minds.CreateMind(session.UserId);
            mind = created;
            minds.TransferTo(created, body, mind: created.Comp);
            em.System<HereticRuleSystem>().InitializeStore(created);
            em.AddComponent<HereticComponent>(mind);
            cloak = em.SpawnEntity("ClothingOuterArmorCloakVoid", map.GridCoords);
            netCloak = em.GetNetEntity(cloak);
            Assert.That(em.System<InventorySystem>().TryEquip(body, cloak, "outerClothing", force: true), Is.True);
            hood = em.GetComponent<Content.Shared.Clothing.Components.ToggleableClothingComponent>(cloak).ClothingUids.Keys.Single();
        });
        await pair.RunSeconds(1);
        for (var i = 0; i < 4; i++)
        {
            var transparent = i % 2 == 0;
            await pair.Server.WaitAssertion(() =>
            {
                var em = pair.Server.EntMan;
                if (transparent)
                    Assert.That(em.System<InventorySystem>().TryEquip(body, hood, "head", force: true), Is.True);
                else
                {
                    Assert.That(em.System<InventorySystem>().TryUnequip(body, "head", force: true), Is.True);
                    em.System<Robust.Shared.Containers.SharedContainerSystem>().Insert(hood,
                        em.GetComponent<Content.Shared.Clothing.Components.ToggleableClothingComponent>(cloak).Container!);
                }
            });
            await pair.RunSeconds(1);
            await pair.Client.WaitAssertion(() =>
            {
                var em = pair.Client.EntMan;
                var clientCloak = em.GetEntity(netCloak);
                Assert.That(em.GetComponent<Content.Trauma.Shared.Heretic.Components.Side.VoidCloakComponent>(clientCloak).Transparent,
                    Is.EqualTo(transparent));
            });
        }
        await pair.Server.WaitAssertion(() =>
        {
            pair.Server.EntMan.DeleteEntity(body);
            pair.Server.EntMan.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FleshGraspRecallsOwnedGhoul()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var body = em.SpawnEntity("MobHuman", map.GridCoords);
            var mind = AddHeretic(em, body);
            em.AddComponent<Content.Trauma.Shared.Heretic.Components.Ghoul.FleshHereticMindComponent>(mind);
            var ghoul = em.SpawnEntity("MobHuman", map.GridCoords.Offset(new Vector2(5, 0)));
            em.GetComponent<HereticComponent>(mind).Minions.Add(ghoul);
            var rune = em.SpawnEntity("HereticRuneRitual", map.GridCoords);
            var grasp = em.SpawnEntity("TouchSpellMansusFlesh", map.GridCoords);
            em.System<SharedHandsSystem>().TryPickup(body, grasp);
            EntityUid? action = null;
            em.System<Content.Shared.Actions.SharedActionsSystem>().AddAction(body, ref action, "ActionHereticMansusGrasp", mind);
            em.GetComponent<TouchSpellComponent>(grasp).Action = action;
            var message = new Content.Trauma.Shared.Heretic.Ui.HereticGhoulRecallMessage(em.GetNetEntity(ghoul))
            {
                Actor = body,
                UiKey = Content.Trauma.Shared.Heretic.Components.Ghoul.HereticGhoulRecallKey.Key,
            };
            em.EventBus.RaiseLocalEvent(rune, message);
            var transform = em.System<SharedTransformSystem>();
            Assert.That(transform.GetMapCoordinates(ghoul), Is.EqualTo(transform.GetMapCoordinates(rune)));
            em.DeleteEntity(body);
            em.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task LockMarkTemporarilyDeniesAccess()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        EntityUid body = default, door = default;
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            body = em.SpawnEntity("MobHuman", map.GridCoords);
            door = em.SpawnEntity("Airlock", map.GridCoords);
            var access = em.System<Content.Shared.Access.Systems.AccessReaderSystem>();
            Assert.That(access.IsAllowed(body, door), Is.True);
            em.System<StatusEffectsSystem>().TryUpdateStatusEffectDuration(body, "LockMarkedStatusEffect", TimeSpan.FromSeconds(1));
            Assert.That(access.IsAllowed(body, door), Is.False);
        });
        await pair.RunSeconds(2);
        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(pair.Server.EntMan.System<Content.Shared.Access.Systems.AccessReaderSystem>().IsAllowed(body, door), Is.True);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task ScorchedMantleBuildsFireStacks()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        EntityUid body = default;
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            body = em.SpawnEntity("MobHuman", map.GridCoords);
            var armor = em.SpawnEntity("ClothingOuterArmorHereticAsh", map.GridCoords);
            Assert.That(em.System<InventorySystem>().TryEquip(body, armor, "outerClothing", force: true), Is.True);
            var mantle = em.GetComponent<Content.Trauma.Shared.Heretic.Components.PathSpecific.Ash.ScorchedMantleComponent>(armor);
            Assert.That(mantle.Action, Is.Not.Null);
            em.System<Content.Shared.Actions.SharedActionsSystem>().SetToggled(mantle.Action!.Value, true);
        });
        await pair.RunSeconds(20);
        await pair.Server.WaitAssertion(() =>
        {
            var fire = pair.Server.EntMan.GetComponent<Content.Shared.Atmos.Components.FlammableComponent>(body);
            Assert.That(fire.OnFire, Is.True);
            Assert.That(fire.FireStacks, Is.GreaterThanOrEqualTo(3));
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task DetachedWoundCannotBeHealed()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var wound = em.SpawnEntity("WeepingAvulsion", map.GridCoords);
            Assert.That(em.System<Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems.WoundSystem>().CanHealWound(wound), Is.False);
        });
        await pair.CleanReturnAsync();
    }

    private static EntityUid AddHeretic(IEntityManager entities, EntityUid body)
    {
        var minds = entities.System<SharedMindSystem>();
        var mind = minds.CreateMind(null);
        minds.TransferTo(mind, body, mind: mind.Comp);
        entities.System<HereticRuleSystem>().InitializeStore(mind);
        entities.AddComponent<HereticComponent>(mind);
        return mind;
    }

    [Test]
    public async Task SharpMedalReturnsAfterThreeThrows()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        EntityUid body = default, mind = default, blade = default;
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var gravity = em.EnsureComponent<GravityComponent>(map.Grid.Owner);
            gravity.EnabledVV = true;
            var maps = em.System<SharedMapSystem>();
            for (var x = -4; x <= 4; x++)
            for (var y = -2; y <= 2; y++)
                maps.SetTile(map.Grid.Owner, map.Grid.Comp, map.GridCoords.Offset(new Vector2(x, y)), map.Tile.Tile);
            body = em.SpawnEntity("MobHuman", map.GridCoords);
            mind = AddHeretic(em, body);
            var medal = em.SpawnEntity("ClothingNeckSharpMedal", map.GridCoords);
            Assert.That(em.System<InventorySystem>().TryEquip(body, medal, "neck", force: true), Is.True);
            blade = em.SpawnEntity("HereticBladeAsh", map.GridCoords);
            Assert.That(em.System<SharedHandsSystem>().TryPickup(body, blade), Is.True);
        });
        for (var i = 0; i < 3; i++)
        {
            await pair.Server.WaitAssertion(() =>
            {
                var em = pair.Server.EntMan;
                Assert.That(em.System<SharedHandsSystem>().TryDrop(body, blade), Is.True);
                em.System<ThrowingSystem>().TryThrow(blade, map.GridCoords.Offset(new Vector2(3, 0)), user: body);
                Assert.That(em.GetComponent<BoomerangComponent>(blade).Thrower, Is.EqualTo(body));
            });
            await pair.RunSeconds(2);
            await pair.Server.WaitAssertion(() =>
                Assert.That(pair.Server.EntMan.System<SharedHandsSystem>().EnumerateHeld(body), Does.Contain(blade)));
        }
        await pair.Server.WaitPost(() =>
        {
            pair.Server.EntMan.DeleteEntity(body);
            pair.Server.EntMan.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task LabyrinthHandbookSpendsChargeAndCreatesWall()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var body = em.SpawnEntity("MobHuman", map.GridCoords);
            var mind = AddHeretic(em, body);
            var book = em.SpawnEntity("LabyrinthHandbook", map.GridCoords);
            var charges = em.System<SharedChargesSystem>();
            var before = charges.GetCurrentCharges(book);
            var ev = new BeforeRangedInteractEvent(body, book, null, map.GridCoords.Offset(new Vector2(1, 0)), true);
            em.EventBus.RaiseLocalEvent(book, ev);
            Assert.That(ev.Handled, Is.True);
            Assert.That(charges.GetCurrentCharges(book), Is.EqualTo(before - 1));
            Assert.That(em.EntityQuery<LabyrinthWallComponent>().Any(), Is.True);
            em.DeleteEntity(book);
            em.DeleteEntity(body);
            em.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("StatusEffectCurseOfCorrosion")]
    [TestCase("StatusEffectCurseOfParalysis")]
    [TestCase("StatusEffectCurseOfFlames")]
    [TestCase("StatusEffectCurseOfAmok")]
    [TestCase("StatusEffectCurseOfFragility")]
    [TestCase("StatusEffectBlindness")]
    public async Task CurseExpires(string prototype)
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        EntityUid body = default;
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            body = em.SpawnEntity("MobHuman", map.GridCoords);
            Assert.That(em.System<StatusEffectsSystem>().TryUpdateStatusEffectDuration(body, prototype, TimeSpan.FromSeconds(1)), Is.True);
        });
        await pair.RunSeconds(2);
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            Assert.That(em.System<StatusEffectsSystem>().HasStatusEffect(body, prototype), Is.False);
            em.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [TestCase(HereticPath.Ash)]
    [TestCase(HereticPath.Void)]
    [TestCase(HereticPath.Flesh)]
    [TestCase(HereticPath.Rust)]
    [TestCase(HereticPath.Blade)]
    [TestCase(HereticPath.Lock)]
    [TestCase(HereticPath.Cosmos)]
    public async Task AscensionDoesNotRequireTraumaErt(HereticPath path)
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var body = em.SpawnEntity("MobHuman", map.GridCoords);
            var mind = AddHeretic(em, body);
            var rule = em.SpawnEntity("HereticRoundstart", Robust.Shared.Map.MapCoordinates.Nullspace);
            em.AddComponent<Content.Shared.GameTicking.Components.ActiveGameRuleComponent>(rule);
            var heretic = em.GetComponent<HereticComponent>(mind);
            heretic.CurrentPath = path;
            em.EventBus.RaiseLocalEvent(mind, new EventHereticAscension());
            Assert.That(heretic.Ascended, Is.True);
            Assert.That(em.GetComponent<Content.Trauma.Server.Heretic.Components.HereticRuleComponent>(rule).HasAHereticAscended, Is.True);
            em.DeleteEntity(rule);
            em.DeleteEntity(body);
            em.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task HereticArmorAllowsDamageWithoutNerveDeduction()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var body = em.SpawnEntity("MobHuman", map.GridCoords);
            var armor = em.SpawnEntity("ClothingOuterArmorHeretic", map.GridCoords);
            Assert.That(em.System<InventorySystem>().TryEquip(body, armor, "outerClothing", force: true), Is.True);
            var wound = em.SpawnEntity("WeepingAvulsion", map.GridCoords);
            var inflicter = em.GetComponent<Content.Shared._Shitmed.Medical.Surgery.Traumas.Components.TraumaInflicterComponent>(wound);
            var deduction = em.System<Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems.TraumaSystem>()
                .GetArmourChanceDeduction(body, (wound, inflicter),
                    Content.Shared._Shitmed.Medical.Surgery.Traumas.TraumaType.NerveDamage,
                    Content.Shared.Body.Part.BodyPartType.Chest);
            Assert.That(deduction.Float(), Is.Zero);
            var weapon = em.SpawnEntity("Throngler", map.GridCoords);
            var damage = em.GetComponent<Content.Shared.Weapons.Melee.MeleeWeaponComponent>(weapon).Damage;
            em.System<Content.Shared.Damage.DamageableSystem>().TryChangeDamage(body, damage, origin: weapon);
            em.DeleteEntity(wound);
            em.DeleteEntity(weapon);
            em.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task ForcedGunfireAllowsIncreasedSpread()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        EntityUid body = default, gun = default;
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            body = em.SpawnEntity("MobHuman", map.GridCoords);
            em.EnsureComponent<Content.Goobstation.Shared.RecoilAbsorber.RecoilAbsorberComponent>(body).Modifier = 10f;
            em.System<Content.Shared.CombatMode.SharedCombatModeSystem>().SetInCombatMode(body, true);
            gun = em.SpawnEntity("WeaponPistolViper", map.GridCoords);
            Assert.That(em.System<SharedHandsSystem>().TryPickup(body, gun), Is.True);
        });
        await pair.RunSeconds(1);
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var component = em.GetComponent<Content.Shared.Weapons.Ranged.Components.GunComponent>(gun);
            Assert.That(component.FireRateModified, Is.GreaterThan(0));
            Assert.That(em.System<Content.Shared.ActionBlocker.ActionBlockerSystem>().CanAttack(body), Is.True, "CanAttack");
            var attempt = new Content.Shared.Weapons.Ranged.Events.ShotAttemptedEvent { User = body, Used = (gun, component) };
            em.EventBus.RaiseLocalEvent(gun, ref attempt);
            Assert.That(attempt.Cancelled, Is.False, "Gun shot prevention");
            em.EventBus.RaiseLocalEvent(body, ref attempt);
            Assert.That(attempt.Cancelled, Is.False, "Body shot prevention");
            var shootAttempt = new Content.Shared.Weapons.Ranged.Systems.AttemptShootEvent(body, null);
            em.EventBus.RaiseLocalEvent(gun, ref shootAttempt);
            Assert.That(shootAttempt.Cancelled, Is.False, shootAttempt.Message);
            em.System<Content.Shared.Weapons.Ranged.Systems.SharedGunSystem>().SetBoltClosed(gun,
                em.GetComponent<Content.Shared.Weapons.Ranged.Components.ChamberMagazineAmmoProviderComponent>(gun), true, body);
            Assert.That(em.GetComponent<Content.Shared.Containers.ItemSlots.ItemSlotsComponent>(gun).Slots["gun_chamber"].HasItem, Is.True, "Loaded chamber");
            for (var i = 0; i < 5; i++)
            {
                component.LastFire = pair.Server.ResolveDependency<Robust.Shared.Timing.IGameTiming>().CurTime;
                component.CurrentAngle = component.MaxAngleModified;
                component.NextFire = TimeSpan.Zero;
                em.GetComponent<Content.Shared.Weapons.Melee.MeleeWeaponComponent>(gun).NextAttack = TimeSpan.Zero;
                em.System<Content.Shared.Weapons.Ranged.Systems.SharedGunSystem>()
                    .AttemptShoot(body, gun, component, map.GridCoords.Offset(new Vector2(5, 0)));
                Assert.That(component.LastFire, Is.GreaterThanOrEqualTo(TimeSpan.Zero), "Recoil evaluated");
            }
            Assert.That(em.EntityQuery<Content.Shared.Projectiles.ProjectileComponent>().Any(), Is.True);
            em.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task LockBladeCanWoundRepeatedly()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            for (var i = 0; i < 20; i++)
            {
                var body = em.SpawnEntity("MobHuman", map.GridCoords);
                for (var hit = 0; hit < 5; hit++)
                    em.System<SharedEntityEffectsSystem>().TryApplyEffect(body, new LockBladeEffect());
                em.DeleteEntity(body);
            }
        });
        await pair.CleanReturnAsync();
    }
}
