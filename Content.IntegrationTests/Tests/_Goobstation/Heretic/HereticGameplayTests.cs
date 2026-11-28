using System.Linq;
using Content.Server.Fluids.EntitySystems;
using Content.Goobstation.Shared.SpaceWhale;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.Inventory;
using Content.Shared.Throwing;
using Content.Goobstation.Shared.Boomerang;
using Content.Trauma.Shared.Heretic.Systems;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;
using Content.Shared.Doors.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;
using Content.Shared.Roles;
using Content.Trauma.Shared.Heretic.Events;
using Content.Trauma.Shared.Tackle;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;
using Content.Trauma.Server.Heretic.Systems.PathSpecific;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffectNew;
using Content.Trauma.Server.Heretic.Systems;
using Content.Trauma.Shared.Heretic.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture]
public sealed class HereticGameplayTests
{
    [Test]
    public async Task WieldedLionhunterEnablesThermalOverlay()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        EntityUid body = default;
        EntityUid mind = default;
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var session = pair.Server.ResolveDependency<ISharedPlayerManager>().Sessions.Single();
            var minds = entities.System<SharedMindSystem>();
            var created = minds.CreateMind(session.UserId);
            mind = created;
            minds.TransferTo(created, body, mind: created.Comp);
            entities.System<HereticRuleSystem>().InitializeStore(created);
            entities.AddComponent<HereticComponent>(mind);
            var rifle = entities.SpawnEntity("WeaponBoltActionLionhunter", map.GridCoords);
            Assert.That(entities.System<Content.Shared.Hands.EntitySystems.SharedHandsSystem>().TryPickup(body, rifle), Is.True);
            Assert.That(entities.System<Content.Shared.Wieldable.SharedWieldableSystem>().TryWield(rifle,
                entities.GetComponent<Content.Shared.Wieldable.Components.WieldableComponent>(rifle), body), Is.True);
        });
        await pair.RunSeconds(2);
        await pair.Client.WaitAssertion(() =>
        {
            var overlays = pair.Client.ResolveDependency<Robust.Client.Graphics.IOverlayManager>();
            Assert.That(overlays.TryGetOverlay<Content.Goobstation.Client.Overlays.ThermalVisionOverlay>(out var overlay), Is.True);
            Assert.That(overlay!.Comp, Is.Not.Null);
            Assert.That(overlay.Comp!.ThermalShader, Is.Null);
        });
        await pair.Server.WaitAssertion(() =>
        {
            pair.Server.EntMan.DeleteEntity(body);
            pair.Server.EntMan.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("MobShattered")]
    [TestCase("MobShatteredLock")]
    [TestCase("MobGhoulMirrorMaid")]
    public async Task HereticMobClientPreviewLoads(string prototype)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        await pair.Client.WaitAssertion(() =>
        {
            var entities = pair.Client.EntMan;
            var preview = entities.SpawnEntity(prototype, Robust.Shared.Map.MapCoordinates.Nullspace);
            entities.DeleteEntity(preview);
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("AshPath")]
    [TestCase("BladePath")]
    [TestCase("CosmosPath")]
    [TestCase("FleshPath")]
    [TestCase("LockPath")]
    [TestCase("RustPath")]
    [TestCase("VoidPath")]
    [TestCase("HereticKnowledge")]
    public async Task HereticGuideFitsWindow(string page)
    {
        await using var pair = await PoolManager.GetServerClient();
        await pair.Client.WaitAssertion(() =>
        {
            var resources = pair.Client.ResolveDependency<Robust.Shared.ContentPack.IResourceManager>();
            var parser = pair.Client.ResolveDependency<Content.Client.Guidebook.DocumentParsingManager>();
            using var reader = resources.ContentFileReadText(new Robust.Shared.Utility.ResPath($"/ServerInfo/_Trauma/Guidebook/Antagonist/Heretic/{page}.xml"));
            using var document = new Content.Client.Guidebook.Richtext.Document();
            Assert.That(parser.TryAddMarkup(document, reader.ReadToEnd()), Is.True);
            foreach (var width in new[] { 300f, 450f, 600f, 800f })
                document.Measure(new System.Numerics.Vector2(width, float.PositiveInfinity));
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("MobGhoulMirrorMaid")]
    [TestCase("MobGhoulFireShark")]
    [TestCase("MobGhoulAshSpirit")]
    [TestCase("MobGhoulRustWalker")]
    public async Task EldritchSummonCanDieFromDamage(string prototype)
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var mob = entities.SpawnEntity(prototype, map.GridCoords);
            var blunt = pair.Server.ProtoMan.Index<DamageTypePrototype>("Blunt");
            entities.System<DamageableSystem>().TryChangeDamage(mob, new DamageSpecifier(blunt, 5000), ignoreResistances: true);
            Assert.That(entities.GetComponent<MobStateComponent>(mob).CurrentState, Is.EqualTo(MobState.Dead));
            entities.DeleteEntity(mob);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task BladeArenaGivesHumanParticipantABlade()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        EntityUid body = default;
        EntityUid arena = default;
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var maps = entities.System<SharedMapSystem>();
            for (var x = -3; x <= 3; x++)
            for (var y = -3; y <= 3; y++)
                maps.SetTile(map.Grid.Owner, map.Grid.Comp,
                    map.GridCoords.Offset(new System.Numerics.Vector2(x, y)), map.Tile.Tile);
            entities.SpawnEntity("WindowDirectional", map.GridCoords.Offset(new System.Numerics.Vector2(1, 0)));
            entities.SpawnEntity("WallSolid", map.GridCoords.Offset(new System.Numerics.Vector2(-1, 0)));
            body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var spawned = entities.System<BladeArenaSystem>().TrySpawnArena(map.GridCoords, "HereticArena", "PlatingRoseStone", 1, 2);
            Assert.That(spawned, Is.Not.Null);
            arena = spawned!.Value;
        });
        await pair.RunSeconds(1);
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            Assert.That(entities.TryGetComponent<HereticArenaParticipantComponent>(body, out var participant), Is.True);
            Assert.That(entities.EntityExists(participant!.Weapon), Is.True);
            entities.DeleteEntity(arena);
            entities.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("Mirror")]
    [TestCase("Window")]
    public async Task MirrorMaidCanEnterMirrorWalk(string surfacePrototype)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        EntityUid maid = default;
        EntityUid surface = default;
        EntityUid mind = default;
        EntityUid action = default;
        EntityUid jaunt = default;
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            maid = entities.SpawnEntity("MobGhoulMirrorMaid", map.GridCoords);
            surface = entities.SpawnEntity(surfacePrototype, map.GridCoords);
            var session = pair.Server.ResolveDependency<ISharedPlayerManager>().Sessions.Single();
            var minds = entities.System<SharedMindSystem>();
            var created = minds.CreateMind(session.UserId);
            mind = created;
            minds.TransferTo(created, maid, mind: created.Comp);
            action = entities.SpawnEntity("ActionMirrorJaunt", map.GridCoords);
            entities.System<SharedActionsSystem>().PerformAction(maid, (action, entities.GetComponent<ActionComponent>(action)));
            Assert.That(created.Comp.OwnedEntity, Is.Not.Null.And.Not.EqualTo(maid));
            jaunt = created.Comp.OwnedEntity!.Value;
        });
        await pair.RunSeconds(1);
        await pair.Server.WaitPost(() =>
        {
            var entities = pair.Server.EntMan;
            entities.System<SharedMindSystem>().WipeMind(mind, entities.GetComponent<MindComponent>(mind));
            entities.DeleteEntity(jaunt);
            entities.DeleteEntity(maid);
            entities.DeleteEntity(surface);
            entities.DeleteEntity(action);
            entities.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FleshWormDamageSynchronizesToConnectedClient()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        EntityUid worm = default;
        EntityUid mind = default;
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            worm = entities.SpawnEntity("MobHereticFleshAscend", map.GridCoords);
            var session = pair.Server.ResolveDependency<ISharedPlayerManager>().Sessions.Single();
            var minds = entities.System<SharedMindSystem>();
            var created = minds.CreateMind(session.UserId);
            mind = created;
            minds.TransferTo(created, worm, mind: created.Comp);
            var blunt = pair.Server.ProtoMan.Index<DamageTypePrototype>("Blunt");
            entities.System<DamageableSystem>().TryChangeDamage(worm, new DamageSpecifier(blunt, 1000), ignoreResistances: true);
        });
        await pair.RunSeconds(2);
        await pair.Client.WaitAssertion(() =>
        {
            var uid = pair.ToClientUid(worm);
            Assert.That(pair.Client.EntMan.EntityExists(uid), Is.True);
        });
        await pair.Server.WaitPost(() =>
        {
            var entities = pair.Server.EntMan;
            entities.System<SharedMindSystem>().WipeMind(mind, entities.GetComponent<MindComponent>(mind));
            entities.DeleteEntity(worm);
            entities.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task CosmicArmorEnablesDashOnlyOnCosmicField()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var armor = entities.SpawnEntity("ClothingOuterArmorHereticCosmos", map.GridCoords);
            Assert.That(entities.System<InventorySystem>().TryEquip(body, armor, "outerClothing", force: true), Is.True);
            var attempt = new TackleEvent(body, new());
            entities.EventBus.RaiseLocalEvent(body, ref attempt);
            Assert.That(attempt.Sources, Is.Empty);
            var field = entities.SpawnEntity("WallFieldCosmic", map.GridCoords);
            attempt = new TackleEvent(body, new());
            entities.EventBus.RaiseLocalEvent(body, ref attempt);
            Assert.That(attempt.Sources, Has.Count.EqualTo(1));
            entities.DeleteEntity(field);
            entities.DeleteEntity(armor);
            entities.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task ConnectedCrewMemberCanBeSelectedForSacrifice()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var crew = entities.SpawnEntity("MobHuman", map.GridCoords);
            var session = pair.Server.ResolveDependency<ISharedPlayerManager>().Sessions.Single();
            var minds = entities.System<SharedMindSystem>();
            var crewMind = minds.CreateMind(session.UserId);
            minds.TransferTo(crewMind, crew, mind: crewMind.Comp);
            entities.System<SharedRoleSystem>().MindAddJobRole(crewMind, jobPrototype: "Passenger");
            var hereticMind = minds.CreateMind(null);
            entities.System<HereticRuleSystem>().InitializeStore(hereticMind);
            var heretic = entities.AddComponent<HereticComponent>(hereticMind);
            var reroll = new EventHereticRerollTargets();
            entities.EventBus.RaiseLocalEvent(hereticMind, reroll);
            Assert.That(heretic.SacrificeTargets.Select(target => target.Entity), Does.Contain(entities.GetNetEntity(crew)));
            minds.WipeMind(crewMind, crewMind.Comp);
            entities.DeleteEntity(crew);
            entities.DeleteEntity(crewMind);
            entities.DeleteEntity(hereticMind);
        });
        await pair.CleanReturnAsync();
    }

    [TestCase(0f)]
    [TestCase(2f)]
    public async Task SerpentclaveTrapCanOpenAndGrapple(float distance)
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        EntityUid door = default;
        EntityUid body = default;
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            door = entities.SpawnEntity("Airlock", map.GridCoords);
            entities.AddComponent<LockTrappedDoorComponent>(door);
        });
        await pair.RunSeconds(1);
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            body = entities.SpawnEntity("MobHuman", map.GridCoords.Offset(new System.Numerics.Vector2(distance, 0)));
            Assert.That(entities.System<SharedDoorSystem>().TryOpen(door, user: body), Is.True);
            Assert.That(entities.GetComponent<LockTrappedDoorComponent>(door).GrappleTarget, Is.EqualTo(body));
            Assert.That(entities.GetComponent<DoorComponent>(door).State, Is.Not.EqualTo(DoorState.Closed));
        });
        await pair.RunSeconds(2);
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            Assert.That(entities.EntityExists(door), Is.True);
            entities.DeleteEntity(door);
            entities.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task SharpMedalEnablesThrownBladeAndAuraFollowsMind()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        EntityUid body = default;
        EntityUid other = default;
        EntityUid mind = default;
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            body = entities.SpawnEntity("MobHuman", map.GridCoords);
            other = entities.SpawnEntity("MobHuman", map.GridCoords);
            var session = pair.Server.ResolveDependency<ISharedPlayerManager>().Sessions.Single();
            var minds = entities.System<SharedMindSystem>();
            var created = minds.CreateMind(session.UserId);
            mind = created;
            minds.TransferTo(created, body, mind: created.Comp);
            entities.System<HereticRuleSystem>().InitializeStore(created);
            var heretic = entities.AddComponent<HereticComponent>(mind);
            heretic.Ascended = true;
            entities.System<HereticSystem>().UpdateHereticAura(body);
            Assert.That(entities.HasComponent<HereticAuraComponent>(body), Is.True);
            Assert.That(entities.HasComponent<HereticAuraComponent>(other), Is.False);
            var medal = entities.SpawnEntity("ClothingNeckSharpMedal", map.GridCoords);
            Assert.That(entities.System<InventorySystem>().TryEquip(body, medal, "neck", force: true), Is.True);
            var blade = entities.SpawnEntity("HereticBladeAsh", map.GridCoords);
            entities.System<ThrowingSystem>().TryThrow(blade, map.GridCoords.Offset(new System.Numerics.Vector2(3, 0)), user: body);
            Assert.That(entities.HasComponent<BoomerangComponent>(blade), Is.True);
            Assert.That(entities.GetComponent<BoomerangComponent>(blade).Thrower, Is.EqualTo(body));
            entities.DeleteEntity(blade);
            entities.DeleteEntity(medal);
            var star = entities.SpawnEntity("ActionHereticStarBlast", map.GridCoords);
            var actions = entities.System<SharedActionsSystem>();
            var cast = (WorldTargetActionEvent) actions.GetEvent(star)!;
            cast.Target = map.GridCoords.Offset(new System.Numerics.Vector2(3, 0));
            actions.PerformAction(body, (star, entities.GetComponent<ActionComponent>(star)), cast);
            var projectile = entities.GetComponent<StarBlastActionComponent>(star).Projectile;
            Assert.That(entities.EntityExists(projectile), Is.True);
            entities.DeleteEntity(projectile);
            entities.DeleteEntity(star);
            minds.TransferTo(mind, other);
        });
        await pair.RunSeconds(1);
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            Assert.That(entities.HasComponent<HereticAuraComponent>(body), Is.False);
            Assert.That(entities.HasComponent<HereticAuraComponent>(other), Is.True);
            entities.System<SharedMindSystem>().WipeMind(mind, entities.GetComponent<MindComponent>(mind));
            entities.DeleteEntity(body);
            entities.DeleteEntity(other);
            entities.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RustSmokeDamagesBorgsWithoutBloodstream()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var borg = entities.SpawnEntity("BorgChassisGeneric", map.GridCoords);
            var smoke = entities.SpawnEntity("Smoke", map.GridCoords);
            var system = entities.System<SmokeSystem>();
            system.StartSmoke(smoke, new Solution("EldritchRust", 30), 10, 0);
            system.SmokeReact(borg, smoke);
            Assert.That(entities.GetComponent<DamageableComponent>(borg).TotalDamage.Float(), Is.GreaterThan(0));
            entities.DeleteEntity(smoke);
            entities.DeleteEntity(borg);
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("CrucibleSoul", "StatusEffectCrucibleSoul")]
    [TestCase("DuskAndDawn", "StatusEffectDuskAndDawn")]
    [TestCase("WoundedSoldier", "StatusEffectWoundedSoldier")]
    [TestCase("NewbornEther", "StatusEffectNewbornEther")]
    public async Task PotionMetabolismAppliesToHeretic(string reagent, string effect)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        EntityUid body = default;
        EntityUid mind = default;
        await server.WaitPost(() =>
        {
            var entities = server.EntMan;
            body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var session = server.ResolveDependency<ISharedPlayerManager>().Sessions.Single();
            var minds = entities.System<SharedMindSystem>();
            var created = minds.CreateMind(session.UserId);
            mind = created;
            minds.TransferTo(created, body, mind: created.Comp);
            entities.System<HereticRuleSystem>().InitializeStore(created);
            entities.AddComponent<HereticComponent>(mind);
            var solution = new Solution(reagent, 5);
            var prototype = server.ProtoMan.Index<ReagentPrototype>(reagent);
            var metabolism = prototype.Metabolisms!["Drink"];
            var args = new EntityEffectReagentArgs(body, entities, null, solution, 5, prototype, null, 1);
            foreach (var condition in metabolism.Conditions!)
                Assert.That(condition.Condition(args), Is.True, condition.GetType().Name);
            foreach (var condition in metabolism.Effects[0].Conditions!)
                Assert.That(condition.Condition(args), Is.True, condition.GetType().Name);
            Assert.That(entities.System<SharedBloodstreamSystem>().TryAddToChemicals(body, solution), Is.True);
        });
        await pair.RunSeconds(3);
        await server.WaitAssertion(() =>
        {
            var entities = server.EntMan;
            Assert.That(entities.System<StatusEffectsSystem>().HasStatusEffect(body, effect), Is.True, reagent);
            entities.System<SharedMindSystem>().WipeMind(mind, entities.GetComponent<MindComponent>(mind));
            entities.DeleteEntity(body);
            entities.DeleteEntity(mind);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FleshWormLosesSegmentsAndDiesFromDamage()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        EntityUid worm = default;
        var originalCount = 0;
        await server.WaitPost(() => worm = server.EntMan.SpawnEntity("MobHereticFleshAscend", map.GridCoords));
        await server.WaitRunTicks(3);
        await server.WaitAssertion(() =>
        {
            var entities = server.EntMan;
            var tail = entities.GetComponent<TailedEntityComponent>(worm);
            originalCount = tail.TailSegments.Count;
            Assert.That(originalCount, Is.GreaterThan(0));
            var damage = entities.System<DamageableSystem>();
            var blunt = server.ProtoMan.Index<DamageTypePrototype>("Blunt");
            damage.TryChangeDamage(worm, new DamageSpecifier(blunt, 1000), ignoreResistances: true);
            Assert.That(entities.GetComponent<DamageableComponent>(worm).TotalDamage.Float(), Is.EqualTo(1000f));
        });
        await server.WaitRunTicks(2);
        await server.WaitAssertion(() =>
        {
            var entities = server.EntMan;
            var tail = entities.GetComponent<TailedEntityComponent>(worm);
            Assert.That(tail.TailSegments.Count, Is.LessThan(originalCount));
            var damage = entities.System<DamageableSystem>();
            var blunt = server.ProtoMan.Index<DamageTypePrototype>("Blunt");
            damage.TryChangeDamage(worm, new DamageSpecifier(blunt, 4000), ignoreResistances: true);
            Assert.That(entities.GetComponent<MobStateComponent>(worm).CurrentState, Is.EqualTo(MobState.Dead));
            entities.DeleteEntity(worm);
        });
        await pair.CleanReturnAsync();
    }
}
