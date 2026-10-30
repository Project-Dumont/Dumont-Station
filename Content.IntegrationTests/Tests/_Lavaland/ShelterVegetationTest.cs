using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.IntegrationTests.Pair;
using Content.Server._Lavaland.DeployableGrid;
using Content.Shared._Lavaland.DeployableGrid;
using Content.Shared.Interaction.Events;
using Robust.Client.Timing;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests._Lavaland;

[TestFixture]
public sealed class ShelterVegetationTest
{
    /// <summary>
    /// Both shelter sizes ignore and remove local vegetation, without clearing outside their footprint.
    /// </summary>
    [TestCase("ShelterCapsule", 5)]
    [TestCase("ShelterCapsuleLuxury", 7)]
    public async Task DeploymentClearsOnlyCoveredVegetation(string prototype, int size)
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await CreatePlanet(pair);
        var plants = new List<EntityUid>();
        EntityUid outside = default;
        EntityUid fringe = default;

        await server.WaitAssertion(() =>
        {
            var species = new[] { "FloraAshBush", "FloraAshSprouts", "FloraPaleSprouts", "FloraAshYellowFlowers", "FloraMossPale", "SerakaMushroom", "Lavashroom" };
            for (var i = 0; i < species.Length; i++)
            {
                var plant = entities.SpawnEntity(species[i], new EntityCoordinates(map.MapUid, new Vector2(19.5f + i % 3, 19.5f + i / 3)));
                Assert.That(entities.GetComponent<TransformComponent>(plant).Anchored, Is.True);
                Assert.That(entities.HasComponent<ShelterClearableComponent>(plant), Is.True);
                plants.Add(plant);
            }

            outside = entities.SpawnEntity("FloraAshBush", new EntityCoordinates(map.MapUid, new Vector2(28.5f, 28.5f)));
            // Adjacent vegetation must not be removed by the shelter's cleanup.
            fringe = entities.SpawnEntity("FloraAshSprouts", new EntityCoordinates(map.MapUid, new Vector2(20f - (size + 1) / 2 + 0.5f, 20.5f)));
            var capsule = entities.SpawnEntity(prototype, new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            Assert.That(server.System<DeployableGridSystem>().TryDeployShelterCapsule((capsule, entities.GetComponent<DeployableGridComponent>(capsule))), Is.True);
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            foreach (var plant in plants)
                Assert.That(entities.EntityExists(plant), Is.False);
            Assert.That(entities.EntityExists(outside), Is.True);
            Assert.That(entities.EntityExists(fringe), Is.True, "Vegetation outside the placed shelter must survive.");
            entities.DeleteEntity(map.MapUid);
        });
        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// Rocks and built structures still block deployment; failed attempts do not remove plants.
    /// Removing the blocker allows the same capsule to retry against the updated map.
    /// </summary>
    [TestCase("WallRockBasalt")]
    [TestCase("Airlock")]
    public async Task BlockedAttemptPreservesVegetationAndCanRetry(string obstruction)
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await CreatePlanet(pair);
        EntityUid plant = default, blocker = default, capsule = default;

        await server.WaitAssertion(() =>
        {
            plant = entities.SpawnEntity("FloraAshBush", new EntityCoordinates(map.MapUid, new Vector2(19.5f, 20.5f)));
            blocker = entities.SpawnEntity(obstruction, new EntityCoordinates(map.MapUid, new Vector2(21.5f, 20.5f)));
            Assert.That(entities.GetComponent<TransformComponent>(blocker).Anchored, Is.True);
            capsule = entities.SpawnEntity("ShelterCapsule", new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            Assert.That(server.System<DeployableGridSystem>().TryDeployShelterCapsule((capsule, entities.GetComponent<DeployableGridComponent>(capsule))), Is.False);
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(entities.EntityExists(plant), Is.True);
            Assert.That(entities.EntityExists(blocker), Is.True);
            entities.DeleteEntity(blocker);
        });
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() => Assert.That(server.System<DeployableGridSystem>().TryDeployShelterCapsule((capsule, entities.GetComponent<DeployableGridComponent>(capsule))), Is.True));
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() =>
        {
            Assert.That(entities.EntityExists(plant), Is.False);
            entities.DeleteEntity(map.MapUid);
        });
        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// All passable basalt decorations permit deployment and retain their ground/radiation properties.
    /// Vegetation under the same shelter is still removed.
    /// </summary>
    [TestCase("ShelterCapsule")]
    [TestCase("ShelterCapsuleLuxury")]
    public async Task PassableBasaltDoesNotBlockOrDisappear(string prototype)
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await CreatePlanet(pair);
        var basalt = new List<EntityUid>();
        EntityUid plant = default;
        await server.WaitAssertion(() =>
        {
            var variants = new[] { "BasaltOne", "BasaltTwo", "BasaltThree", "BasaltFour", "BasaltFive", "BasaltRandom" };
            for (var i = 0; i < variants.Length; i++)
            {
                var decoration = entities.SpawnEntity(variants[i], new EntityCoordinates(map.MapUid, new Vector2(19.5f + i % 3, 19.5f + i / 3)));
                Assert.That(entities.GetComponent<TransformComponent>(decoration).Anchored, Is.True);
                Assert.That(entities.GetComponent<ShelterClearableComponent>(decoration).RemoveOnDeploy, Is.False);
                basalt.Add(decoration);
            }
            plant = entities.SpawnEntity("SerakaMushroom", new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            var capsule = entities.SpawnEntity(prototype, new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            var system = server.System<DeployableGridSystem>();
            var ent = (capsule, entities.GetComponent<DeployableGridComponent>(capsule));
            Assert.That(system.GetDeploymentCheck(ent).BlockingEntities, Is.Empty);
            Assert.That(system.TryDeployShelterCapsule(ent), Is.True);
        });
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() =>
        {
            foreach (var decoration in basalt)
                Assert.That(entities.EntityExists(decoration), Is.True, "Passable basalt must not be cleared.");
            Assert.That(entities.EntityExists(plant), Is.False);
            entities.DeleteEntity(map.MapUid);
        });
        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// Clearing vegetation must not bypass the existing planet-only deployment restriction.
    /// </summary>
    [Test]
    public async Task OrdinaryGridStillRejectsDeployment()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var map = await pair.CreateTestMap();
        await server.WaitAssertion(() =>
        {
            var plant = server.EntMan.SpawnEntity("FloraAshBush", map.GridCoords);
            var capsule = server.EntMan.SpawnEntity("ShelterCapsule", map.GridCoords);
            Assert.That(server.System<DeployableGridSystem>().TryDeployShelterCapsule((capsule, server.EntMan.GetComponent<DeployableGridComponent>(capsule))), Is.False);
            Assert.That(server.EntMan.IsQueuedForDeletion(plant), Is.False);
            server.EntMan.DeleteEntity(map.MapUid);
        });
        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// A clearing exactly the size of either shelter must work even with rocks along every edge.
    /// Exercises rounding on both tile parities and away from tile centers.
    /// </summary>
    [TestCase("ShelterCapsule", 5, 20.5f)]
    [TestCase("ShelterCapsule", 5, 21.5f)]
    [TestCase("ShelterCapsule", 5, 20.25f)]
    [TestCase("ShelterCapsuleLuxury", 7, 20.5f)]
    [TestCase("ShelterCapsuleLuxury", 7, 21.5f)]
    [TestCase("ShelterCapsuleLuxury", 7, 20.25f)]
    public async Task ExactClearingDoesNotIncludeAdjacentRocks(string prototype, int size, float center)
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await CreatePlanet(pair);
        var rocks = new List<EntityUid>();
        var origin = (int) MathF.Round(center - size / 2f);

        await server.WaitAssertion(() =>
        {
            for (var x = origin - 1; x <= origin + size; x++)
            {
                for (var y = origin - 1; y <= origin + size; y++)
                {
                    if (x != origin - 1 && x != origin + size && y != origin - 1 && y != origin + size)
                        continue;
                    rocks.Add(entities.SpawnEntity("WallRockBasalt", new EntityCoordinates(map.MapUid, new Vector2(x + 0.5f, y + 0.5f))));
                }
            }
            var capsule = entities.SpawnEntity(prototype, new EntityCoordinates(map.MapUid, new Vector2(center, center)));
            Assert.That(server.System<DeployableGridSystem>().TryDeployShelterCapsule((capsule, entities.GetComponent<DeployableGridComponent>(capsule))), Is.True);
        });
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() =>
        {
            foreach (var rock in rocks)
                Assert.That(entities.EntityExists(rock), Is.True, "Adjacent rocks must remain intact.");
            var query = entities.EntityQueryEnumerator<MapGridComponent, TransformComponent>();
            var foundShelter = false;
            while (query.MoveNext(out var uid, out var grid, out var xform))
            {
                if (uid == map.MapUid || xform.MapUid != map.MapUid)
                    continue;
                var bounds = server.System<SharedTransformSystem>().GetWorldMatrix(uid).TransformBox(grid.LocalAABB);
                Assert.That(bounds, Is.EqualTo(new Box2(new Vector2(origin, origin), new Vector2(origin + size, origin + size))));
                foundShelter = true;
            }
            Assert.That(foundShelter, Is.True);
            entities.DeleteEntity(map.MapUid);
        });
        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// Builds a map whose grid is the map itself, as required by the planet-only check.
    /// </summary>
    private static async Task<TestMapData> CreatePlanet(TestPair pair)
    {
        var map = await pair.CreateTestMap();
        await pair.Server.WaitPost(() =>
        {
            var entities = pair.Server.EntMan;
            entities.DeleteEntity(map.Grid);
            var grid = entities.AddComponent<MapGridComponent>(map.MapUid);
            var tiles = new List<(Vector2i, Tile)>();
            for (var x = 10; x <= 30; x++)
            {
                for (var y = 10; y <= 30; y++)
                    tiles.Add((new Vector2i(x, y), new Tile(1)));
            }
            pair.Server.System<SharedMapSystem>().SetTiles(map.MapUid, grid, tiles);
        });
        return map;
    }

    /// <summary>
    /// Nearby grids remain forbidden even when the footprint itself contains only clearable plants.
    /// Their entity IDs are available to the preview for highlighting the actual structure.
    /// </summary>
    [TestCase("ShelterCapsule", 5)]
    [TestCase("ShelterCapsuleLuxury", 7)]
    public async Task NearbyGridIsReportedAsBlocker(string prototype, int size)
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await CreatePlanet(pair);
        EntityUid capsule = default, nearby = default, plant = default;
        await server.WaitPost(() =>
        {
            capsule = entities.SpawnEntity(prototype, new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            plant = entities.SpawnEntity("SerakaMushroom", new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            var maps = server.System<SharedMapSystem>();
            var grid = maps.CreateGridEntity(map.MapId);
            nearby = grid.Owner;
            maps.SetTile(grid, Vector2i.Zero, new Tile(1));
            server.System<SharedTransformSystem>().SetCoordinates(nearby, new EntityCoordinates(map.MapUid, new Vector2(20.5f + size / 2f + 1, 20)));
        });
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() =>
        {
            var system = server.System<DeployableGridSystem>();
            var ent = (capsule, entities.GetComponent<DeployableGridComponent>(capsule));
            var check = system.GetDeploymentCheck(ent);
            Assert.That(check.NearbyGrids, Does.Contain(nearby));
            Assert.That(check.BlockingEntities, Is.Empty);
            Assert.That(check.FailureMessage, Is.EqualTo("shelter-capsule-fail-near-grid"));
            Assert.That(system.TryDeployShelterCapsule(ent), Is.False);
            Assert.That(entities.IsQueuedForDeletion(plant), Is.False);
            entities.DeleteEntity(map.MapUid);
        });
        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// The preview exists only during inspection of a held capsule and updates as blockers change.
    /// </summary>
    [Test]
    public async Task InspectionPreviewTracksObstaclesAndClosesWithTooltip()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var server = pair.Server;
        var client = pair.Client;
        var map = await CreatePlanet(pair);
        EntityUid user = default, capsule = default, rock = default;
        await server.WaitAssertion(() =>
        {
            user = server.EntMan.SpawnEntity("MobHuman", new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            capsule = server.EntMan.SpawnEntity("ShelterCapsule", new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            Assert.That(server.System<Content.Server.Hands.Systems.HandsSystem>().TryPickup(user, capsule), Is.True);
            server.PlayerMan.SetAttachedEntity(pair.Player!, user);
        });
        await pair.RunTicksSync(20);
        var clientUser = pair.ToClientUid(user);
        var clientCapsule = pair.ToClientUid(capsule);
        await client.WaitAssertion(() =>
        {
            var system = client.System<Content.Client._Lavaland.DeployableGrid.DeployableGridSystem>();
            Assert.That(system.TryGetPreview(out _, out _), Is.False, "Holding alone must not enable the preview.");
            client.System<Content.Client.Examine.ExamineSystem>().OpenTooltip(clientUser, clientCapsule);
            Assert.That(system.TryGetPreview(out var inspected, out var check), Is.True);
            Assert.That(inspected, Is.EqualTo(clientCapsule));
            Assert.That(check.CanDeploy, Is.True);
            Assert.That(check.Bounds.Size, Is.EqualTo(new Vector2(5, 5)));
        });
        await server.WaitPost(() => rock = server.EntMan.SpawnEntity("WallRockBasalt", new EntityCoordinates(map.MapUid, new Vector2(21.5f, 20.5f))));
        await pair.RunTicksSync(20);
        var clientRock = pair.ToClientUid(rock);
        await client.WaitAssertion(() =>
        {
            var system = client.System<Content.Client._Lavaland.DeployableGrid.DeployableGridSystem>();
            Assert.That(system.TryGetPreview(out _, out var check), Is.True);
            Assert.That(check.CanDeploy, Is.False);
            Assert.That(check.BlockingEntities, Does.Contain(clientRock));
        });
        await server.WaitPost(() => server.EntMan.DeleteEntity(rock));
        await pair.RunTicksSync(20);
        await client.WaitAssertion(() =>
        {
            var system = client.System<Content.Client._Lavaland.DeployableGrid.DeployableGridSystem>();
            Assert.That(system.TryGetPreview(out _, out var check), Is.True);
            Assert.That(check.CanDeploy, Is.True, "Mining the blocker must update the existing preview.");
            var ui = client.ResolveDependency<IUserInterfaceManager>();
            var popup = ui.ModalRoot.Children.OfType<Popup>().Single(control => control.Children.Any(child => child.Name == "ExaminePopupPanel"));
            popup.Close();
            Assert.That(system.TryGetPreview(out _, out _), Is.False, "Closing inspection must hide the preview.");
        });
        await server.WaitPost(() =>
        {
            server.PlayerMan.SetAttachedEntity(pair.Player!, null);
            server.EntMan.DeleteEntity(map.MapUid);
        });
        await pair.RunTicksSync(20);
        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// Replaying a failed client prediction must not repeat its original no-space popup.
    /// </summary>
    [Test]
    public async Task NoSpacePopupIsNotRepeatedByPrediction()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var server = pair.Server;
        var client = pair.Client;
        var map = await CreatePlanet(pair);
        EntityUid user = default, capsule = default;
        await server.WaitPost(() =>
        {
            user = server.EntMan.SpawnEntity("MobHuman", new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            capsule = server.EntMan.SpawnEntity("ShelterCapsule", new EntityCoordinates(map.MapUid, new Vector2(20.5f, 20.5f)));
            server.EntMan.SpawnEntity("WallRockBasalt", new EntityCoordinates(map.MapUid, new Vector2(21.5f, 20.5f)));
            server.PlayerMan.SetAttachedEntity(pair.Player!, user);
        });
        await pair.RunTicksSync(20);
        var clientUser = pair.ToClientUid(user);
        var clientCapsule = pair.ToClientUid(capsule);
        await client.WaitAssertion(() =>
        {
            var popups = client.System<Content.Client.Popups.PopupSystem>();
            var timing = client.ResolveDependency<IClientGameTiming>();
            client.EntMan.EventBus.RaiseLocalEvent(clientCapsule, new UseInHandEvent(clientUser));
            var message = Loc.GetString("shelter-capsule-fail-no-space");
            var popup = popups.WorldLabels.Single(label => label.Text == message);
            Assert.That(popup.Repeats, Is.EqualTo(1));
            using (timing.StartPastPredictionArea())
            {
                for (var i = 0; i < 8; i++)
                    client.EntMan.EventBus.RaiseLocalEvent(clientCapsule, new UseInHandEvent(clientUser));
            }
            Assert.That(popup.Repeats, Is.EqualTo(1), "Past predictions must not duplicate the failure message.");
        });
        await server.WaitPost(() =>
        {
            server.PlayerMan.SetAttachedEntity(pair.Player!, null);
            server.EntMan.DeleteEntity(map.MapUid);
        });
        await pair.RunTicksSync(360);
        await pair.CleanReturnAsync();
    }
}
