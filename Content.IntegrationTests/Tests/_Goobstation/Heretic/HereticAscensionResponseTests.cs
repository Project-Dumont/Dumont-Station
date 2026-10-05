using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.RoundEnd;
using Content.Server.Power.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Inventory;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Server.AlertLevel;
using Content.Shared.Station.Components;
using Robust.Shared.Prototypes;
using Content.Trauma.Server.Heretic.Components;
using Content.Trauma.Server.Heretic.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture]
public sealed class HereticAscensionResponseTests
{
    [Test]
    public async Task AscensionLoadsInquisitorsAndCallsEvacuationOnlyOnce()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { DummyTicker = false, Dirty = true });
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var rule = em.SpawnEntity("HereticRoundstart", MapCoordinates.Nullspace);
            em.AddComponent<ActiveGameRuleComponent>(rule);
            var response = em.System<HereticRuleSystem>();
            response.SpawnERTOnAscension();

            Assert.That(em.GetComponent<HereticRuleComponent>(rule).HasAHereticAscended, Is.True);
            var evacuation = em.System<RoundEndSystem>();
            Assert.That(evacuation.IsRoundEndRequested(), Is.False);
            Assert.That(em.EntityQuery<MetaDataComponent, RuleGridsComponent>(true)
                .Any(e => e.Item1.EntityPrototype?.ID == "SpawnVatican"), Is.False);
        });
        await pair.RunSeconds(19);
        await pair.Server.WaitAssertion(() =>
            Assert.That(pair.Server.EntMan.System<RoundEndSystem>().IsRoundEndRequested(), Is.False));
        await pair.RunSeconds(2);
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var response = em.System<HereticRuleSystem>();
            var evacuation = em.System<RoundEndSystem>();
            Assert.That(evacuation.IsRoundEndRequested(), Is.True);
            var levels = em.EntityQuery<AlertLevelComponent, StationDataComponent>(true).ToArray();
            Assert.That(levels, Is.Not.Empty);
            Assert.That(levels.All(e => e.Item1.CurrentLevel == "cataclysm"), Is.True);
            var species = pair.Server.ProtoMan.Index<RandomHumanoidSettingsPrototype>("HereticInquisitorERT").SpeciesBlacklist;
            Assert.That(species, Does.Contain("Synth"));
            Assert.That(species, Does.Contain("IPC"));
            var deadline = evacuation.ExpectedCountdownEnd;
            var ertRules = em.EntityQuery<MetaDataComponent, RuleGridsComponent>(true)
                .Where(e => e.Item1.EntityPrototype?.ID == "SpawnVatican").ToArray();
            Assert.That(ertRules, Has.Length.EqualTo(1));
            Assert.That(ertRules[0].Item2.MapGrids, Has.Count.EqualTo(1));

            var grid = ertRules[0].Item2.MapGrids.Single();
            var inventory = em.System<InventorySystem>();
            var inquisitors = 0;
            var leaders = 0;
            foreach (var (inv, transform) in em.EntityQuery<InventoryComponent, TransformComponent>(true))
            {
                if (transform.GridUid != grid || !inventory.TryGetSlotEntity(inv.Owner, "outerClothing", out var armor))
                    continue;
                if (em.GetComponent<MetaDataComponent>(armor.Value).EntityPrototype?.ID == "ClothingModsuitChestplateInquisitory")
                {
                    Assert.That(em.GetComponent<HumanoidAppearanceComponent>(inv.Owner).Species.Id,
                        Is.Not.EqualTo("Synth").And.Not.EqualTo("IPC"));
                    Assert.That(inventory.TryGetSlotEntity(inv.Owner, "back", out var control), Is.True);
                    Assert.That(em.GetComponent<MetaDataComponent>(control.Value).EntityPrototype?.ID,
                        Is.EqualTo("ClothingModsuitInquisitory"));
                    if (inventory.TryGetSlotEntity(inv.Owner, "neck", out var cloak)
                        && em.GetComponent<MetaDataComponent>(cloak.Value).EntityPrototype?.ID == "ClothingNeckCloakInquisitor")
                        leaders++;
                    Assert.That(inventory.TryGetSlotEntity(inv.Owner, "ears", out var headset), Is.True);
                    Assert.That(em.GetComponent<MetaDataComponent>(headset.Value).EntityPrototype?.ID, Is.EqualTo("ClothingHeadsetVatican"));
                    Assert.That(inventory.TryGetSlotEntity(inv.Owner, "pocket1", out var pistol), Is.True);
                    Assert.That(em.GetComponent<MetaDataComponent>(pistol.Value).EntityPrototype?.ID, Is.EqualTo("VaticanDueProcess"));
                    Assert.That(inventory.TryGetSlotEntity(inv.Owner, "suitstorage", out var themis), Is.True);
                    Assert.That(em.GetComponent<MetaDataComponent>(themis.Value).EntityPrototype?.ID, Is.EqualTo("VaticanThemis"));
                    inquisitors++;
                }
            }
            Assert.That(inquisitors, Is.EqualTo(5));
            Assert.That(leaders, Is.EqualTo(1));
            Assert.That(evacuation.RecallLocked, Is.True);
            evacuation.CancelRoundEndCountdown(checkCooldown: false);
            Assert.That(evacuation.IsRoundEndRequested(), Is.True);
            Assert.That(evacuation.ExpectedCountdownEnd, Is.EqualTo(deadline));

            response.SpawnERTOnAscension();
            Assert.That(evacuation.ExpectedCountdownEnd, Is.EqualTo(deadline));
            Assert.That(em.EntityQuery<MetaDataComponent, RuleGridsComponent>(true)
                .Count(e => e.Item1.EntityPrototype?.ID == "SpawnVatican"), Is.EqualTo(1));
        });
        // Let the ship's electrical network settle, then check beyond the APC trip delay.
        for (var sample = 0; sample < 4; sample++)
        {
            await pair.RunSeconds(30);
            await pair.Server.WaitAssertion(() =>
            {
                var em = pair.Server.EntMan;
                var grid = em.EntityQuery<MetaDataComponent, RuleGridsComponent>(true)
                    .Single(e => e.Item1.EntityPrototype?.ID == "SpawnVatican").Item2.MapGrids.Single();
                var apcs = em.EntityQuery<ApcComponent, TransformComponent>(true)
                    .Where(e => e.Item2.GridUid == grid).ToArray();
                Assert.That(apcs.Length, Is.GreaterThan(1));
                Assert.That(apcs.All(e => e.Item1.MainBreakerEnabled && !e.Item1.TripFlag), Is.True,
                    "The Drakon's APCs must stay on after startup.");
                var equipment = em.EntityQuery<MetaDataComponent, ApcPowerReceiverComponent, TransformComponent>(true)
                    .Where(e => e.Item3.GridUid == grid &&
                        (e.Item1.EntityPrototype?.ID.StartsWith("Thruster") == true ||
                         e.Item1.EntityPrototype?.ID == "ComputerShuttle")).ToArray();
                Assert.That(equipment, Is.Not.Empty);
                foreach (var (meta, power, transform) in equipment)
                    Assert.That(power.Powered, Is.True,
                        $"{meta.EntityPrototype?.ID} at {transform.Coordinates} has no power.");
            });
        }
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RoundRestartCancelsPendingVaticanResponse()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { DummyTicker = false, Dirty = true });
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var rule = em.SpawnEntity("HereticRoundstart", MapCoordinates.Nullspace);
            em.AddComponent<ActiveGameRuleComponent>(rule);
            em.System<HereticRuleSystem>().SpawnERTOnAscension();
            em.EventBus.RaiseEvent(EventSource.Local, new Content.Shared.GameTicking.RoundRestartCleanupEvent());
        });
        await pair.RunSeconds(21);
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            Assert.That(em.System<RoundEndSystem>().IsRoundEndRequested(), Is.False);
            Assert.That(em.EntityQuery<MetaDataComponent, RuleGridsComponent>(true)
                .Any(e => e.Item1.EntityPrototype?.ID == "SpawnVatican"), Is.False);
        });
        await pair.CleanReturnAsync();
    }
}
