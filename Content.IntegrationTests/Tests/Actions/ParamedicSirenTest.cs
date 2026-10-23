using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Light.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;
using PointLightComponent = Robust.Server.GameObjects.PointLightComponent;

namespace Content.IntegrationTests.Tests.Actions;

[TestFixture]
public sealed class ParamedicSirenTest
{
    [Test]
    public async Task SirenActionTogglesSoundAndLight()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var map = await pair.CreateTestMap();
        var entities = server.EntMan;
        var actions = server.System<SharedActionsSystem>();
        var inventory = server.System<InventorySystem>();

        await server.WaitAssertion(() =>
        {
            var wearer = entities.SpawnEntity("MobHuman", map.GridCoords);
            var suit = entities.SpawnEntity("ClothingOuterHardsuitVoidParamed", map.GridCoords);
            Assert.That(inventory.TryEquip(wearer, suit, "outerClothing", force: true), Is.True);

            var clothing = entities.GetComponent<ToggleClothingComponent>(suit);
            var action = actions.GetAction(clothing.ActionEntity)!.Value;
            Assert.That(action.Comp.Icon, Is.TypeOf<SpriteSpecifier.Rsi>());
            Assert.That(((SpriteSpecifier.Rsi) action.Comp.Icon!).RsiState, Is.EqualTo("icon-siren"));
            actions.SetUseDelay(action, null);
            Assert.That(action.Comp.Sound, Is.Null);
            Assert.That(((SoundPathSpecifier) clothing.SoundActivate!).Path.ToString(), Is.EqualTo("/Audio/Items/flashlight_on.ogg"));
            Assert.That(((SoundPathSpecifier) clothing.SoundDeactivate!).Path.ToString(), Is.EqualTo("/Audio/Items/flashlight_off.ogg"));
            var toggle = entities.GetComponent<ItemToggleComponent>(suit);
            Assert.That(toggle.SoundActivate, Is.Null, "Only the button action should play a click.");
            Assert.That(toggle.SoundDeactivate, Is.Null, "Unequipping must stop the siren without a click.");

            var sound = entities.GetComponent<ItemToggleActiveSoundComponent>(suit);
            var light = entities.GetComponent<PointLightComponent>(suit);
            var rotation = entities.GetComponent<RotatingLightComponent>(suit);
            Assert.That(light.Enabled, Is.False);
            Assert.That(sound.PlayingStream, Is.Null);

            // Exercise the actual action, including its event and provider wiring.
            actions.PerformAction(wearer, action);
            Assert.That(light.Enabled, Is.True);
            Assert.That(rotation.Enabled, Is.True);
            Assert.That(sound.PlayingStream, Is.Not.Null);
            var stream = entities.GetComponent<AudioComponent>(sound.PlayingStream!.Value);
            Assert.That(stream.FileName, Is.EqualTo("/Audio/Effects/Vehicle/ambulancesiren.ogg"));
            Assert.That(stream.Params.Loop, Is.True);

            actions.PerformAction(wearer, action);
            Assert.That(light.Enabled, Is.False);
            Assert.That(sound.PlayingStream, Is.Null);

            actions.PerformAction(wearer, action);
            Assert.That(light.Enabled, Is.True);
            Assert.That(inventory.TryUnequip(wearer, "outerClothing", force: true), Is.True);
            Assert.That(light.Enabled, Is.False);
            Assert.That(sound.PlayingStream, Is.Null);
        });

        await server.WaitPost(() => entities.DeleteEntity(map.MapUid));
        await pair.CleanReturnAsync();
    }
}
