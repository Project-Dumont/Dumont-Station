using System.Collections.Generic;
using System.Numerics;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared._Dumont.RPD;
using Content.Shared.Atmos.Components;
using Content.Shared.Charges.Components;
using Content.Shared.RCD.Components;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests._Dumont.RPD;

public sealed class RpdLayerSelectorTest : InteractionTest
{
    [TestPrototypes]
    private const string Prototypes = @"
- type: rcd
  id: RpdLayerTestPipe
  mode: ConstructObject
  prototype: GasPipeFourway
  cost: 1
  delay: 0.5
  fx: EffectRCDConstruct0

- type: entity
  id: RpdLayerTestTool
  parent: RPDAdvanced
  components:
  - type: RCD
    availablePrototypes: [RpdLayerTestPipe]
";

    [Test]
    public async Task AltClickCyclesAndNetworksLayer()
    {
        var tool = await PlaceInHands("RPDAdvanced");
        var serverTool = SEntMan.GetEntity(tool);
        var expectedLayers = new AtmosPipeLayer?[]
        {
            AtmosPipeLayer.Primary,
            AtmosPipeLayer.Secondary,
            AtmosPipeLayer.Tertiary,
            null,
        };

        Assert.That(SEntMan.GetComponent<RpdLayerSelectorComponent>(serverTool).SelectedLayer, Is.Null);

        foreach (var expected in expectedLayers)
        {
            await Server.WaitAssertion(() =>
            {
                Assert.That(InteractSys.AltInteract(SPlayer, serverTool), Is.True);
                Assert.That(SEntMan.GetComponent<RpdLayerSelectorComponent>(serverTool).SelectedLayer, Is.EqualTo(expected));
            });
            await RunTicks(5);
            await Client.WaitAssertion(() =>
                Assert.That(CEntMan.GetComponent<RpdLayerSelectorComponent>(ToClient(tool)).SelectedLayer, Is.EqualTo(expected)));
        }
    }

    [TestCase(1, AtmosPipeLayer.Primary)]
    [TestCase(2, AtmosPipeLayer.Secondary)]
    [TestCase(3, AtmosPipeLayer.Tertiary)]
    public async Task FixedLayerIgnoresMousePosition(int clicks, AtmosPipeLayer expected)
    {
        var tool = SEntMan.GetEntity(await PlaceInHands("RPDAdvanced"));
        await Server.WaitPost(() =>
        {
            for (var i = 0; i < clicks; i++)
                InteractSys.AltInteract(SPlayer, tool);
        });

        foreach (var offset in new[] { Vector2.Zero, new Vector2(0.35f, 0.35f), new Vector2(-0.35f, -0.35f) })
        {
            await Interact(null, GetTargetCoordinates().Offset(offset));
            await Server.WaitAssertion(() =>
            {
                var pipes = GetPipes();
                Assert.That(pipes, Has.Count.EqualTo(1));
                Assert.That(pipes[0].Layer, Is.EqualTo(expected));
                SEntMan.DeleteEntity(pipes[0].Uid);
            });
        }
    }

    [Test]
    public async Task PendingConstructionKeepsItsLayer()
    {
        var tool = SEntMan.GetEntity(await PlaceInHands("RpdLayerTestTool"));
        await Server.WaitAssertion(() =>
        {
            var coords = GetTargetCoordinates();
            Assert.That(InteractSys.AltInteract(SPlayer, tool), Is.True);
            InteractSys.UserInteraction(SPlayer, coords, null);
            Assert.That(InteractSys.AltInteract(SPlayer, tool), Is.True);
            InteractSys.UserInteraction(SPlayer, coords, null);
        });
        await RunTicks(60);

        await Server.WaitAssertion(() =>
        {
            var pipes = GetPipes();
            Assert.That(pipes.ConvertAll(pipe => pipe.Layer),
                Is.EquivalentTo(new[] { AtmosPipeLayer.Primary, AtmosPipeLayer.Secondary }));
        });
    }

    [TestCase("RCD")]
    [TestCase("RPD")]
    [TestCase("RPDRecharging")]
    [TestCase("RCDProtoCE")]
    public async Task ExistingToolsDoNotHaveLayerSelector(string prototype)
    {
        var tool = SEntMan.GetEntity(await PlaceInHands(prototype));
        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.HasComponent<RpdLayerSelectorComponent>(tool), Is.False);
            Assert.That(InteractSys.AltInteract(SPlayer, tool), Is.False);
        });
    }

    [Test]
    public async Task AdvancedRpdKeepsSpriteRecipesAndCharges()
    {
        var regular = await PlaceInHands("RPD");
        await RunTicks(5);
        var regularRcd = SEntMan.GetComponent<RCDComponent>(SEntMan.GetEntity(regular));
        var regularCharges = SEntMan.GetComponent<LimitedChargesComponent>(SEntMan.GetEntity(regular)).MaxCharges;
        string spritePath = null;
        await Client.WaitAssertion(() =>
        {
            spritePath = CEntMan.GetComponent<SpriteComponent>(ToClient(regular)).BaseRSI?.Path.ToString();
            Assert.That(spritePath, Is.Not.Null);
        });

        var advanced = await PlaceInHands("RPDAdvanced");
        await RunTicks(5);
        await Server.WaitAssertion(() =>
        {
            var uid = SEntMan.GetEntity(advanced);
            Assert.That(SEntMan.GetComponent<RCDComponent>(uid).AvailablePrototypes, Is.EquivalentTo(regularRcd.AvailablePrototypes));
            Assert.That(SEntMan.GetComponent<LimitedChargesComponent>(uid).MaxCharges, Is.EqualTo(regularCharges));
        });
        await Client.WaitAssertion(() =>
            Assert.That(CEntMan.GetComponent<SpriteComponent>(ToClient(advanced)).BaseRSI?.Path.ToString(), Is.EqualTo(spritePath)));
    }

    private EntityCoordinates GetTargetCoordinates()
    {
        return Transform.WithEntityId(SEntMan.GetCoordinates(TargetCoords), MapData.Grid.Owner);
    }

    private List<(EntityUid Uid, AtmosPipeLayer Layer)> GetPipes()
    {
        var pipes = new List<(EntityUid Uid, AtmosPipeLayer Layer)>();
        var query = SEntMan.EntityQueryEnumerator<AtmosPipeLayersComponent>();
        while (query.MoveNext(out var uid, out var pipe))
        {
            if (Transform.GetGrid(uid) == MapData.Grid.Owner)
                pipes.Add((uid, pipe.CurrentPipeLayer));
        }

        return pipes;
    }
}
