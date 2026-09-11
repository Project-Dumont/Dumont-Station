using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture]
public sealed class HereticSpawnTests
{
    [TestCase("MobGhoulStalker")]
    [TestCase("MobGhoulStalkerLock")]
    [TestCase("MobGhoulProphet")]
    [TestCase("MobGhoulProphetLock")]
    [TestCase("MobGhoulShattered")]
    [TestCase("MobShatteredLock")]
    [TestCase("MobHereticFleshAscend")]
    public async Task SummonStartsAlive(string prototype)
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        EntityUid entity = default;
        await server.WaitPost(() => entity = server.EntMan.SpawnEntity(prototype, map.GridCoords));
        await server.WaitRunTicks(3);
        await server.WaitAssertion(() =>
        {
            Assert.That(server.EntMan.Deleted(entity), Is.False);
            var state = server.EntMan.GetComponent<MobStateComponent>(entity);
            Assert.That(state.CurrentState, Is.EqualTo(MobState.Alive));
            server.EntMan.DeleteEntity(entity);
        });
        await pair.CleanReturnAsync();
    }
}
