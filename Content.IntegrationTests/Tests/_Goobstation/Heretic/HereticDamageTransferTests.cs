using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture]
public sealed class HereticDamageTransferTests
{
    [Test]
    public async Task SmallDamageSurvivesRepeatedTransfers()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        await server.WaitAssertion(() =>
        {
            var entMan = server.EntMan;
            var first = entMan.SpawnEntity(null, map.MapCoords);
            var second = entMan.SpawnEntity(null, map.MapCoords);
            var firstDamage = entMan.AddComponent<DamageableComponent>(first);
            var secondDamage = entMan.AddComponent<DamageableComponent>(second);
            entMan.AddComponent<MobStateComponent>(first);
            entMan.AddComponent<MobStateComponent>(second);
            entMan.AddComponent<MobThresholdsComponent>(first);
            entMan.AddComponent<MobThresholdsComponent>(second);
            var damageSystem = entMan.System<DamageableSystem>();
            var thresholds = entMan.System<MobThresholdSystem>();
            thresholds.SetMobStateThreshold(first, 100, MobState.Critical);
            thresholds.SetMobStateThreshold(first, 200, MobState.Dead);
            thresholds.SetMobStateThreshold(second, 200, MobState.Critical);
            thresholds.SetMobStateThreshold(second, 300, MobState.Dead);
            var blunt = server.ProtoMan.Index<DamageTypePrototype>("Blunt");
            damageSystem.SetDamage(first, firstDamage, new DamageSpecifier(blunt, FixedPoint2.New(0.1)));

            for (var i = 0; i < 10; i++)
            {
                thresholds.TransferDamage(first, second);
                Assert.That(secondDamage.TotalDamage, Is.EqualTo(FixedPoint2.New(0.2)));
                thresholds.TransferDamage(second, first);
                Assert.That(firstDamage.TotalDamage, Is.EqualTo(FixedPoint2.New(0.1)));
            }
            entMan.DeleteEntity(first);
            entMan.DeleteEntity(second);
        });
        await pair.CleanReturnAsync();
    }
}
