using System.Linq;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared._Dumont.Clothing;
using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameObjects;
using PointLightComponent = Robust.Server.GameObjects.PointLightComponent;

namespace Content.IntegrationTests.Tests.Actions;

[TestFixture]
public sealed class ModsuitSirenTest
{
    /// <summary>
    /// Exercises the real suit and siren actions, power depletion and recovery,
    /// helmet retraction, and unsealing without changing the built-in flashlight.
    /// </summary>
    [Test]
    public async Task SirenRequiresEquippedHelmetAndSuitPower()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var map = await pair.CreateTestMap();
        var entities = server.EntMan;
        var actions = server.System<SharedActionsSystem>();
        var inventory = server.System<InventorySystem>();
        var clothing = server.System<ToggleableClothingSystem>();
        var power = server.System<PowerCellSystem>();
        var batteries = server.System<BatterySystem>();
        EntityUid wearer = default, suit = default, helmet = default, cell = default;
        ModsuitSirenComponent siren = null;
        SealableClothingControlComponent control = null;
        var clicksBeforeShutdown = 0;

        await server.WaitAssertion(() =>
        {
            wearer = entities.SpawnEntity("MobHuman", map.GridCoords);
            suit = entities.SpawnEntity("ClothingModsuitParamedicPowerCell", map.GridCoords);
            Assert.That(inventory.TryEquip(wearer, suit, "back", force: true), Is.True);
            var toggleable = entities.GetComponent<ToggleableClothingComponent>(suit);
            helmet = toggleable.ClothingUids.Single(part => part.Value == "head").Key;
            siren = entities.GetComponent<ModsuitSirenComponent>(helmet);
            Assert.That(siren.Action, Is.Null);

            foreach (var (part, slot) in toggleable.ClothingUids)
                Assert.That(clothing.EquipClothing(wearer, (suit, toggleable), part, slot), Is.True);

            control = entities.GetComponent<SealableClothingControlComponent>(suit);
            Assert.That(power.TryGetBatteryFromSlot(suit, out var battery, out _), Is.True);
            cell = battery!.Value;
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(control.IsCurrentlySealed, Is.False, "The siren must work before sealing the suit.");
            Assert.That(actions.GetAction(siren.Action)?.Comp.AttachedEntity, Is.EqualTo(wearer));
            var action = actions.GetAction(siren.Action)!.Value;
            Assert.That(action.Comp.Container, Is.EqualTo(helmet), "The helmet must own the siren action.");
            Assert.That(action.Comp.RaiseOnUser, Is.False);
            Assert.That(actions.GetEvent(action), Is.TypeOf<ModsuitSirenToggleEvent>());
            Assert.That(action.Comp.Sound, Is.Null, "The siren uses the helmet light's separate on/off sounds.");
            actions.SetUseDelay(action, null);
            var clicks = CountSwitchSounds();
            actions.PerformAction(wearer, action);
            Assert.That(CountSwitchSounds(), Is.EqualTo(clicks + 1));
            Assert.That(actions.GetEvent(action)!.Handled, Is.True, "The siren system must handle its action.");
            AssertSiren(true);
            Assert.That(entities.GetComponent<PointLightComponent>(helmet).Enabled, Is.False);
            actions.PerformAction(wearer, action);
            AssertSiren(false);
            actions.PerformAction(wearer, action);
            AssertSiren(true);
            Assert.That(CountSwitchSounds(), Is.EqualTo(clicks + 3));
            clicksBeforeShutdown = CountSwitchSounds();
            batteries.SetCharge(cell, 0);
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(actions.GetAction(siren.Action)?.Comp.AttachedEntity, Is.Null);
            AssertSiren(false);
            Assert.That(CountSwitchSounds(), Is.EqualTo(clicksBeforeShutdown), "Power loss must not play a switch sound.");
            // Even a stale action reference must not restart an unpowered siren.
            actions.PerformAction(wearer, actions.GetAction(siren.Action)!.Value);
            AssertSiren(false);
            batteries.SetCharge(cell, 1000);
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(actions.GetAction(siren.Action)?.Comp.AttachedEntity, Is.EqualTo(wearer));
            AssertSiren(false);
            actions.PerformAction(wearer, actions.GetAction(siren.Action)!.Value);
            AssertSiren(true);
            inventory.TryUnequip(wearer, "head", force: true);
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(inventory.TryGetSlotEntity(wearer, "head", out _), Is.False);
            Assert.That(actions.GetAction(siren.Action)?.Comp.AttachedEntity, Is.Null);
            AssertSiren(false);
            var toggleable = entities.GetComponent<ToggleableClothingComponent>(suit);
            clothing.EquipClothing(wearer, (suit, toggleable), helmet, "head");
            Assert.That(control.IsCurrentlySealed, Is.False);
            Assert.That(actions.GetAction(siren.Action)?.Comp.AttachedEntity, Is.Null);
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(actions.GetAction(siren.Action)?.Comp.AttachedEntity, Is.EqualTo(wearer));
            actions.PerformAction(wearer, actions.GetAction(siren.Action)!.Value);
            AssertSiren(true);
            actions.PerformAction(wearer, actions.GetAction(control.SealActionEntity)!.Value);
        });
        await pair.RunTicksSync(180);

        await server.WaitAssertion(() =>
        {
            Assert.That(control.IsCurrentlySealed, Is.True);
            Assert.That(actions.GetAction(siren.Action)?.Comp.AttachedEntity, Is.EqualTo(wearer));
            AssertSiren(true);
            actions.PerformAction(wearer, actions.GetAction(control.SealActionEntity)!.Value);
        });
        await pair.RunTicksSync(180);

        await server.WaitAssertion(() =>
        {
            Assert.That(control.IsCurrentlySealed, Is.False);
            Assert.That(actions.GetAction(siren.Action)?.Comp.AttachedEntity, Is.EqualTo(wearer));
            AssertSiren(true);
            var toggleable = entities.GetComponent<ToggleableClothingComponent>(suit);
            foreach (var slot in toggleable.ClothingUids.Values)
                inventory.TryUnequip(wearer, slot, force: true);
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            Assert.That(inventory.TryUnequip(wearer, "back", force: true), Is.True);
            entities.DeleteEntity(suit);
        });
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() => Assert.That(entities.EntityExists(siren.Emitter), Is.False));

        await server.WaitPost(() => entities.DeleteEntity(map.MapUid));
        await pair.CleanReturnAsync();

        void AssertSiren(bool enabled)
        {
            Assert.That(siren.Emitter, Is.Not.Null);
            var emitter = siren.Emitter!.Value;
            var toggle = entities.GetComponent<ItemToggleComponent>(emitter);
            Assert.That(toggle.SoundActivate, Is.Null, "Only the button action should play a click.");
            Assert.That(toggle.SoundDeactivate, Is.Null, "Automatic shutdown must not play a click.");
            Assert.That(entities.GetComponent<PointLightComponent>(emitter).Enabled, Is.EqualTo(enabled));
            Assert.That(entities.GetComponent<ItemToggleComponent>(emitter).Activated, Is.EqualTo(enabled));
            var sound = entities.GetComponent<ItemToggleActiveSoundComponent>(emitter);
            if (!enabled)
            {
                Assert.That(sound.PlayingStream, Is.Null);
                return;
            }

            Assert.That(sound.PlayingStream, Is.Not.Null);
            Assert.That(entities.GetComponent<AudioComponent>(sound.PlayingStream!.Value).Params.Loop, Is.True);
            Assert.That(actions.GetAction(siren.Action)!.Value.Comp.Toggled, Is.True);
        }

        int CountSwitchSounds()
        {
            var count = 0;
            var query = entities.EntityQueryEnumerator<AudioComponent>();
            while (query.MoveNext(out var audio))
            {
                if (audio.FileName is "/Audio/Items/flashlight_on.ogg" or "/Audio/Items/flashlight_off.ogg")
                    count++;
            }
            return count;
        }
    }
}
