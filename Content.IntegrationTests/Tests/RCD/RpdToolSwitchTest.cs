using System.Numerics;
using System.Reflection;
using Content.Client.RCD;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Atmos.Components;
using Content.Shared.Interaction;
using Content.Shared.RCD.Components;
using Content.Shared.RCD;
using Moq;
using Robust.Client.Graphics;
using Robust.Client.Placement;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.RCD;

public sealed class RpdToolSwitchTest : InteractionTest
{
    [Test]
    public async Task SwitchToolsWithoutRotatingCamera()
    {
        var first = await PlaceInHands("RPD");
        await BuildPipe(first, AtmosPipeLayer.Secondary);
        await Drop();

        var second = await PlaceInHands("RPD");
        await BuildPipe(second, AtmosPipeLayer.Secondary);
        await Drop();

        await Pickup(first, deleteHeld: false);
        await BuildPipe(first, AtmosPipeLayer.Tertiary);
        await Drop();

        await Pickup(second, deleteHeld: false);
        await BuildPipe(second, AtmosPipeLayer.Primary);
    }

    [TestCase(Math.PI, 0, AtmosPipeLayer.Tertiary)]
    [TestCase(0, Math.PI, AtmosPipeLayer.Tertiary)]
    [TestCase(0.3, -0.3, AtmosPipeLayer.Secondary)]
    public async Task SwitchingPreservesRotations(double eyeRotation, double gridRotation, AtmosPipeLayer layer)
    {
        await Server.WaitPost(() => Transform.SetLocalRotation(MapData.Grid, gridRotation));
        var first = await PlaceInHands("RPD");
        await BuildPipe(first, layer, eyeRotation, new Vector2(0.9f, 0.4f));
        await Drop();

        var second = await PlaceInHands("RPD");
        await BuildPipe(second, layer, eyeRotation, new Vector2(0.9f, 0.4f));
    }

    [Test]
    public async Task RotationIsSynchronizedWithoutRepeatedMessages()
    {
        var tool = await PlaceInHands("RPD");
        var actualNetwork = Client.ResolveDependency<IEntityNetworkManager>();
        var network = new Mock<IEntityNetworkManager>();
        network.Setup(n => n.SendSystemNetworkMessage(It.IsAny<EntityEventArgs>(), true))
            .Callback<EntityEventArgs, bool>((message, recordReplay) =>
                actualNetwork.SendSystemNetworkMessage(message, recordReplay));

        await RunTicks(5);
        await AlignTool(new Vector2(0.9f, 0.4f), 0.3, network: network.Object);
        await RunTicks(5);
        network.Verify(n => n.SendSystemNetworkMessage(It.IsAny<EntityEventArgs>(), true), Times.Once());

        for (var i = 0; i < 5; i++)
        {
            await AlignTool(new Vector2(0.9f, 0.4f), 0.3, network: network.Object);
            await RunTicks(2);
        }

        network.Verify(n => n.SendSystemNetworkMessage(It.IsAny<EntityEventArgs>(), true), Times.Once());
        await AlignTool(new Vector2(0.9f, 0.4f), 0.6, network: network.Object);
        await RunTicks(5);
        network.Verify(n => n.SendSystemNetworkMessage(It.IsAny<EntityEventArgs>(), true), Times.Exactly(2));

        await Client.WaitPost(() => actualNetwork.SendSystemNetworkMessage(new RPDEyeRotationEvent(tool, 0.9f)));
        await RunTicks(5);
        await Client.WaitAssertion(() => Assert.That(
            CEntMan.GetComponent<RCDComponent>(CEntMan.GetEntity(tool)).LastKnownEyeRotation,
            Is.EqualTo(0.9f)));
        await Server.WaitAssertion(() => Assert.That(
            SEntMan.GetComponent<RCDComponent>(SEntMan.GetEntity(tool)).LastKnownEyeRotation,
            Is.EqualTo(0.9f)));
    }

    [Test]
    public async Task CannotUpdateToolHeldByAnotherEntity()
    {
        var ownTool = await PlaceInHands("RPD");
        await RunTicks(5);
        await AlignTool(new Vector2(0.9f, 0.4f), 0.3);
        await RunTicks(5);

        NetEntity otherTool = default;
        await Server.WaitAssertion(() =>
        {
            var otherUser = SEntMan.SpawnAtPosition("InteractionTestMob", MapData.GridCoords);
            var uid = SEntMan.SpawnAtPosition("RPD", MapData.GridCoords);
            Assert.That(HandSys.TryPickupAnyHand(otherUser, uid), Is.True);
            otherTool = SEntMan.GetNetEntity(uid);
        });
        await RunTicks(5);

        await Client.WaitPost(() => Client.ResolveDependency<IEntityNetworkManager>()
            .SendSystemNetworkMessage(new RPDEyeRotationEvent(otherTool, 0.6f)));
        await RunTicks(5);

        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.GetComponent<RCDComponent>(SEntMan.GetEntity(ownTool)).LastKnownEyeRotation,
                Is.EqualTo(0.3f));
            Assert.That(SEntMan.GetComponent<RCDComponent>(SEntMan.GetEntity(otherTool)).LastKnownEyeRotation,
                Is.Null);
        });
    }

    private async Task AlignTool(Vector2 position, double eyeRotation, AtmosPipeLayer? expectedLayer = null,
        IEntityNetworkManager network = null)
    {
        await Client.WaitAssertion(() =>
        {
            var eyeManager = Client.ResolveDependency<IEyeManager>();
            eyeManager.CurrentEye = new FixedEye
            {
                Position = new MapCoordinates(0.5f, 0.5f, MapId),
                Rotation = new Angle(eyeRotation),
            };
            var placement = (PlacementManager) Client.ResolveDependency<IPlacementManager>();
            Client.System<RCDConstructionGhostSystem>().Update(0);
            var mode = placement.CurrentMode as AlignRPDAtmosPipeLayers;
            Assert.That(mode, Is.Not.Null);
            if (network != null)
            {
                typeof(AlignRPDAtmosPipeLayers).GetField("_entityNetwork", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(mode, network);
            }
            var coords = new EntityCoordinates(MapData.CGridUid, position);
            var viewport = new Mock<IViewportControl>();
            var mapCoords = CEntMan.System<SharedTransformSystem>().ToMapCoordinates(coords);
            viewport.Setup(v => v.PixelToMap(It.IsAny<Vector2>())).Returns(mapCoords);
            var originalViewport = eyeManager.MainViewport;
            eyeManager.MainViewport = viewport.Object;
            try
            {
                mode!.AlignPlacementMode(default);
                if (expectedLayer is { } layer)
                {
                    var proto = Client.ResolveDependency<IPrototypeManager>()
                        .Index<EntityPrototype>(placement.CurrentPermission!.EntityType);
                    Assert.That(proto.TryComp<AtmosPipeLayersComponent>(out var layers, CEntMan.ComponentFactory));
                    Assert.That(layers!.CurrentPipeLayer, Is.EqualTo(layer));
                }
            }
            finally
            {
                eyeManager.MainViewport = originalViewport;
            }
        });
    }

    private async Task BuildPipe(NetEntity tool, AtmosPipeLayer expectedLayer, double eyeRotation = 0, Vector2? position = null)
    {
        var clickPosition = position ?? expectedLayer switch
        {
            AtmosPipeLayer.Primary => new Vector2(0.5f, 0.5f),
            AtmosPipeLayer.Secondary => new Vector2(0.9f, 0.4f),
            AtmosPipeLayer.Tertiary => new Vector2(0.1f, 0.6f),
            _ => throw new ArgumentOutOfRangeException(nameof(expectedLayer)),
        };
        await RunTicks(5);
        await AlignTool(clickPosition, eyeRotation, expectedLayer);
        await RunTicks(5);

        await Server.WaitPost(() =>
        {
            var uid = SEntMan.GetEntity(tool);
            SEntMan.EventBus.RaiseLocalEvent(uid,
                new AfterInteractEvent(SPlayer, uid, null, new EntityCoordinates(MapData.Grid, clickPosition), true));
        });
        await RunTicks(5);

        await Server.WaitAssertion(() =>
        {
            var pipes = SEntMan.AllEntities<AtmosPipeLayersComponent>();
            Assert.That(pipes, Has.Length.EqualTo(1));
            var rotation = SEntMan.GetComponent<RCDComponent>(SEntMan.GetEntity(tool)).LastKnownEyeRotation;
            Assert.That(pipes[0].Comp.CurrentPipeLayer, Is.EqualTo(expectedLayer),
                $"RPD {tool}: stored eye rotation = {rotation?.ToString() ?? "null"}");
            Assert.That(rotation, Is.EqualTo((float) eyeRotation));
            TestContext.Out.WriteLine($"RPD {tool}: {pipes[0].Comp.CurrentPipeLayer}, eye rotation = {rotation}");
            SEntMan.DeleteEntity(pipes[0].Owner);
        });
        await RunTicks(5);
    }
}
