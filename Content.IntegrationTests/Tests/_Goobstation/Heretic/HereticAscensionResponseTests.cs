using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.RoundEnd;
using Content.Shared.GameTicking.Components;
using Content.Shared.Inventory;
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
            Assert.That(evacuation.IsRoundEndRequested(), Is.True);
            var deadline = evacuation.ExpectedCountdownEnd;
            var ertRules = em.EntityQuery<MetaDataComponent, RuleGridsComponent>(true)
                .Where(e => e.Item1.EntityPrototype?.ID == "SpawnHereticInquisitorERT").ToArray();
            Assert.That(ertRules, Has.Length.EqualTo(1));
            Assert.That(ertRules[0].Item2.MapGrids, Has.Count.EqualTo(1));

            var grid = ertRules[0].Item2.MapGrids.Single();
            var inventory = em.System<InventorySystem>();
            var inquisitors = 0;
            foreach (var (inv, transform) in em.EntityQuery<InventoryComponent, TransformComponent>(true))
            {
                if (transform.GridUid != grid || !inventory.TryGetSlotEntity(inv.Owner, "outerClothing", out var armor))
                    continue;
                if (em.GetComponent<MetaDataComponent>(armor.Value).EntityPrototype?.ID == "ClothingOuterArmorInquisitor")
                    inquisitors++;
            }
            Assert.That(inquisitors, Is.EqualTo(5));

            response.SpawnERTOnAscension();
            Assert.That(evacuation.ExpectedCountdownEnd, Is.EqualTo(deadline));
            Assert.That(em.EntityQuery<MetaDataComponent, RuleGridsComponent>(true)
                .Count(e => e.Item1.EntityPrototype?.ID == "SpawnHereticInquisitorERT"), Is.EqualTo(1));
        });
        await pair.CleanReturnAsync();
    }
}
