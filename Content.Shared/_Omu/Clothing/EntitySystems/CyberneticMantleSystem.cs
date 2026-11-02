// SPDX-FileCopyrightText: 2025 MarkerWicker (OmuStation)
// SPDX-License-Identifier: AGPL-3.0-or-later
// Ported from OmuStation PRs #150 and #1613; adapted for Dumont.

using System.Linq;
using Content.Shared._Omu.Clothing.Components;
using Content.Shared._Omu.Clothing;
using Content.Shared._Omu.Traits;
using Content.Shared.Clothing.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Content.Shared.Actions;
using Content.Shared.Inventory;

namespace Content.Shared._Omu.Clothing.EntitySystems;

public sealed class CyberneticMantleSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _itemToggleSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPointLightSystem _lightSystem = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyberneticMantleComponent, BeingEquippedAttemptEvent>(OnBeingEquipped, after: [typeof(ClothingTakesUpExtraSlotsSystem)]);
        SubscribeLocalEvent<CyberneticMantleComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<CyberneticMantleComponent, AboutToEnterToggleableClothingContainerEvent>(OnToggleClothing);
        SubscribeLocalEvent<CyberneticMantleComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<CyberneticMantleComponent, CyberneticMantleLightToggleEvent>(OnToggleLight);
    }

    private void OnGetActions(
        EntityUid uid,
        CyberneticMantleComponent component,
        GetItemActionsEvent args)
    {
        if (!args.IsEquipping)
        {
            args.AddAction(component.LightToggleActionEntity);
            return;
        }

        if (args.SlotFlags != SlotFlags.MASK)
            return;

        args.AddAction(
            ref component.LightToggleActionEntity,
            "ActionToggleCyberneticMantleLight");

        Dirty(uid, component);
    }

    private void OnToggleLight(
        EntityUid uid,
        CyberneticMantleComponent component,
        CyberneticMantleLightToggleEvent args)
    {
        if (args.Handled)
            return;

        var strongLight = !args.Action.Comp.Toggled;

        _lightSystem.SetRadius(uid, strongLight ? 5f : 1.45f);
        _lightSystem.SetEnergy(uid, strongLight ? 0.45f : 0.6f);
        _lightSystem.SetFalloff(uid, strongLight ? 6.8f : 0f);

        _actionsSystem.SetToggled(component.LightToggleActionEntity, strongLight);
        _audio.PlayPredicted(new SoundPathSpecifier(strongLight
            ? "/Audio/Items/flashlight_on.ogg"
            : "/Audio/Items/flashlight_off.ogg"), uid, args.Performer);

        args.Handled = true;
    }

    /// <summary>
    ///     A list of things to do when the mantle is equpped. Enables the mantle's ItemToggleComponent.
    /// </summary>
    private void OnBeingEquipped(Entity<CyberneticMantleComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        // if the client is resetting predicted entities, trying to modify the components on the mantle will result in an exception. So don't do it.
        if (_gameTiming.ApplyingState && _netManager.IsClient)
            return;

        // make sure that the person equipping the mantle is a cybernetic beast
        if (TryComp<CyberneticBeastComponent>(args.EquipTarget, out var _))
        {
            // if the component can be toggled, turn it on when worn
            if (TryComp<ItemToggleComponent>(ent, out var itemToggle))
                _itemToggleSystem.TrySetActive(new Entity<ItemToggleComponent?>(ent.Owner, itemToggle), true, args.EquipTarget, true);

            // get the appearance of the beast
            if (TryComp<HumanoidAppearanceComponent>(args.EquipTarget, out var humanoidAppearance))
            {
                // clamp the beast's eye color to a reasonable brightness; no tiders with stealth mantles allowed
                var eyeColorHsv = Color.ToHsv(humanoidAppearance.EyeColor);
                eyeColorHsv.Z = Math.Clamp(eyeColorHsv.Z, ent.Comp.MinEyeColorLevel, 1.0f);
                var eyeColor = Color.FromHsv(eyeColorHsv);

                // set the eye colour of the mantle to the (clamped) eye colour of the beast
                SetEyeColor(ent, eyeColor);

                // if this visor emits light, make that light take on the clamped eye colour of the beast
                _lightSystem.SetColor(ent, eyeColor);
            }
        }

    }

    /// <summary>
    ///     A list of things to do when the mantle is unequipped. Disables the mantle's ItemToggleComponent.
    /// </summary>>
    private void OnUnequipped(Entity<CyberneticMantleComponent> ent, ref GotUnequippedEvent args)
    {
        // if we try to predict this more than once it'll throw an exception about modifying components while resetting predicted entities.
        if (!_gameTiming.IsFirstTimePredicted && _netManager.IsClient)
            return;

        // if the compnent can be toggled, turn it off when unequipped.
        if (TryComp<ItemToggleComponent>(ent, out var itemToggle))
            _itemToggleSystem.TrySetActive(new Entity<ItemToggleComponent?>(ent.Owner, itemToggle), false, args.Equipee, true);
    }

    /// <summary>
    ///     Sets the colour of the eyes-layer of the mantle to the specified colour.
    /// </summary>
    private void SetEyeColor(Entity<CyberneticMantleComponent> ent, Color color)
    {
        _appearance.SetData(ent, CyberneticMantleVisuals.EyeColor, color);
    }

    /// <summary>
    ///     Tries to place the cybernetic mantle into the player's hands, if a piece of clothing is about to be toggled which would go over the mantle.
    /// </summary>
    /// <remarks>
    ///     We don't want the mantle to go under the clothing for one reason: It's too convenient. Don't need to think about storing your mantle somewhere when the toggleable clothing does it for you.
    /// </remarks>
    private void OnToggleClothing(Entity<CyberneticMantleComponent> ent, ref AboutToEnterToggleableClothingContainerEvent args)
    {
        _handsSystem.PickupOrDrop(args.ClothingParent, ent);
    }

}
