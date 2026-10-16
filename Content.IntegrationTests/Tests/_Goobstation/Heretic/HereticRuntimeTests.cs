using System.Linq;
using Content.Server.Objectives.Systems;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Eye;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.StepTrigger.Systems;
using Content.Trauma.Server.Heretic.Systems;
using Content.Trauma.Server.Objectives.Components;
using Content.Trauma.Shared.Heretic.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Emoting;
using Content.Shared.Movement.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture]
public sealed class HereticRuntimeTests
{
    [Test]
    public async Task PaleFogAppliesItsDebuff()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var fog = entities.SpawnEntity("HereticPaleFog", map.GridCoords);
            var step = new StepTriggeredOffEvent(fog, body);
            entities.EventBus.RaiseLocalEvent(fog, ref step);
            Assert.That(entities.System<StatusEffectsSystem>().HasStatusEffect(body, "PaleFogAffectedStatusEffect"), Is.True);
            entities.DeleteEntity(fog);
            entities.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task ScorchedMantleActionTogglesOnItsOwnEntity()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var uid = entities.SpawnEntity("ActionHereticScorchedMantleToggleFlames", map.GridCoords);
            var action = entities.GetComponent<ActionComponent>(uid);
            var actions = entities.System<SharedActionsSystem>();
            Assert.That(action.RaiseOnAction, Is.True);
            actions.PerformAction(body, (uid, action));
            Assert.That(action.Toggled, Is.True);
            actions.PerformAction(body, (uid, action));
            Assert.That(action.Toggled, Is.False);
            entities.DeleteEntity(uid);
            entities.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("PotionEther", "NewbornEther")]
    [TestCase("PotionCrucibleSoul", "CrucibleSoul")]
    [TestCase("PotionDuskDawn", "DuskAndDawn")]
    [TestCase("PotionWoundedSoldier", "WoundedSoldier")]
    public async Task PotionStartsFilled(string prototype, string reagent)
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var potion = entities.SpawnEntity(prototype, map.GridCoords);
            Assert.That(entities.System<SharedSolutionContainerSystem>().TryGetSolution(potion, "drink", out _, out var solution), Is.True);
            Assert.That(solution!.GetTotalPrototypeQuantity(reagent).Float(), Is.EqualTo(5f));
            entities.DeleteEntity(potion);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RepeatedFlipsCauseVomitAndBlockFurtherFlips()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var flip = pair.Server.ProtoMan.Index<EmotePrototype>("Flip");
            var status = entities.System<StatusEffectsSystem>();
            for (var i = 0; i < 4; i++)
            {
                var emote = new EmoteEvent(flip);
                entities.EventBus.RaiseLocalEvent(body, ref emote);
                Assert.That(status.HasStatusEffect(body, "BlockVomitEmotesStatusEffect"), Is.False);
            }
            var lastEmote = new EmoteEvent(flip);
            entities.EventBus.RaiseLocalEvent(body, ref lastEmote);
            Assert.That(status.HasStatusEffect(body, "BlockVomitEmotesStatusEffect"), Is.True);
            Assert.That(status.HasStatusEffect(body, MovementModStatusSystem.VomitingSlowdown), Is.True);
            var attempt = new BeforeEmoteEvent(body, flip);
            entities.EventBus.RaiseLocalEvent(body, ref attempt);
            Assert.That(attempt.Cancelled, Is.True);
            entities.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RiftConditionsAndObjectiveProgress()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        await server.WaitAssertion(() =>
        {
            var entities = server.EntMan;
            var body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var rift = entities.SpawnEntity("EldritchInfluence", map.GridCoords);
            Assert.That(entities.GetComponent<VisibilityComponent>(rift).Layer,
                Is.EqualTo((ushort) VisibilityFlags.EldritchInfluence));

            entities.System<StutteringSystem>().DoStutter(body, TimeSpan.FromSeconds(5), true);
            Assert.That(entities.HasComponent<StutteringAccentComponent>(body), Is.True);

            var status = entities.System<StatusEffectsSystem>();
            var step = new StepTriggeredOffEvent(rift, body);
            entities.EventBus.RaiseLocalEvent(rift, ref step);
            Assert.That(status.HasStatusEffect(body, "StatusEffectInfluenceXray"), Is.False);

            var session = server.ResolveDependency<ISharedPlayerManager>().Sessions.Single();
            var minds = entities.System<SharedMindSystem>();
            var mind = minds.CreateMind(session.UserId);
            minds.TransferTo(mind, body, mind: mind.Comp);
            entities.System<HereticRuleSystem>().InitializeStore(mind);
            entities.AddComponent<HereticComponent>(mind);
            entities.EventBus.RaiseLocalEvent(rift, ref step);
            Assert.That(status.HasStatusEffect(body, "StatusEffectInfluenceXray"), Is.True);

            foreach (var prototype in new[] { "HereticKnowledgeObjective", "HereticSacrificeObjective", "HereticSacrificeHeadObjective" })
            {
                var objective = entities.SpawnEntity(prototype, map.GridCoords);
                var assigned = new ObjectiveAssignedEvent(mind, mind.Comp);
                entities.EventBus.RaiseLocalEvent(objective, ref assigned);
                var progress = new ObjectiveGetProgressEvent(mind, mind.Comp);
                entities.EventBus.RaiseLocalEvent(objective, ref progress);
                Assert.That(progress.Progress, Is.EqualTo(0f), prototype);
                var target = entities.System<NumberObjectiveSystem>().GetTarget(objective);
                if (entities.TryGetComponent<HereticKnowledgeConditionComponent>(objective, out var knowledge))
                    knowledge.Researched = target;
                else
                    entities.GetComponent<HereticSacrificeConditionComponent>(objective).Sacrificed = target;
                progress = new ObjectiveGetProgressEvent(mind, mind.Comp);
                entities.EventBus.RaiseLocalEvent(objective, ref progress);
                Assert.That(progress.Progress, Is.EqualTo(1f), prototype);
                entities.DeleteEntity(objective);
            }

            minds.WipeMind(mind, mind.Comp);
            entities.DeleteEntity(body);
            entities.DeleteEntity(mind);
            entities.DeleteEntity(rift);
        });
        await pair.CleanReturnAsync();
    }
    [Test]
    public async Task PaleFogRespectsNullrodProtection()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var rod = entities.SpawnEntity("Nullrod", map.GridCoords);
            Assert.That(entities.System<Content.Shared.Hands.EntitySystems.SharedHandsSystem>().TryPickup(body, rod), Is.True);
            var fog = entities.SpawnEntity("HereticPaleFog", map.GridCoords);
            var step = new StepTriggeredOffEvent(fog, body);
            entities.EventBus.RaiseLocalEvent(fog, ref step);
            Assert.That(entities.System<StatusEffectsSystem>().HasStatusEffect(body, "PaleFogAffectedStatusEffect"), Is.False);
            entities.DeleteEntity(fog);
            entities.DeleteEntity(body);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task StorePreservesBodyActionsChargesAndSaleLimits()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        await pair.Server.WaitAssertion(() =>
        {
            var entities = pair.Server.EntMan;
            var body = entities.SpawnEntity("MobHuman", map.GridCoords);
            var minds = entities.System<SharedMindSystem>();
            var mind = minds.CreateMind(null);
            minds.TransferTo(mind, body, mind: mind.Comp);
            var storeEntity = entities.SpawnEntity(null, map.GridCoords);
            var store = entities.AddComponent<Content.Shared.Store.Components.StoreComponent>(storeEntity);
            store.GrantActionsToMind = false;
            store.Balance["CPU"] = 1000;
            var prototype = pair.Server.ProtoMan.Index<Content.Shared.Store.ListingPrototype>("MalfAiOverloadMachine");
            var listing = new Content.Shared.Store.ListingDataWithCostModifiers(prototype);
            listing.SaleLimit = 1;
            listing.AddCostModifier("DumontSales", new() { ["CPU"] = -25 });
            store.FullListingsCatalog = new() { listing };
            store.Categories = listing.Categories;
            var buy = new Content.Shared.Store.StoreBuyListingMessage("MalfAiOverloadMachine", null) { Actor = body };
            entities.EventBus.RaiseLocalEvent(storeEntity, buy);
            Assert.That(listing.PurchaseAmount, Is.EqualTo(1));
            Assert.That(listing.Cost["CPU"], Is.EqualTo(prototype.Cost["CPU"]));
            var actions = entities.GetComponent<ActionsComponent>(body);
            var action = actions.Actions.Single(uid => entities.GetComponent<MetaDataComponent>(uid).EntityPrototype?.ID == "ActionMalfAiOverloadMachine");
            var charges = entities.System<Content.Shared.Charges.Systems.SharedChargesSystem>();
            charges.SetCharges(action, 0);
            entities.EventBus.RaiseLocalEvent(storeEntity, buy);
            Assert.That(listing.PurchaseAmount, Is.EqualTo(2));
            Assert.That(actions.Actions.Count(uid => entities.GetComponent<MetaDataComponent>(uid).EntityPrototype?.ID == "ActionMalfAiOverloadMachine"), Is.EqualTo(1));
            Assert.That(charges.GetCurrentCharges(action), Is.EqualTo(2));
            minds.WipeMind(mind, mind.Comp);
            entities.DeleteEntity(body);
            entities.DeleteEntity(mind);
            entities.DeleteEntity(storeEntity);
        });
        await pair.CleanReturnAsync();
    }

}
