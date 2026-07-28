// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs;
using Content.Trauma.Shared.Silicons.Borgs;
using Content.Trauma.Shared.Silicons.Borgs.Components;
using Robust.Server.GameObjects;

namespace Content.Trauma.Server.Silicons.Borgs;

public sealed partial class BorgDisguiseSystem : SharedBorgDisguiseSystem
{
    private CompName _lightName;

    public override void Initialize()
    {
        base.Initialize();

        _lightName = Factory.CompName<PointLightComponent>();
        SubscribeLocalEvent<BorgDisguiseComponent, BorgDisguiseToggleActionEvent>(OnDisguiseToggle);
        SubscribeLocalEvent<BorgDisguiseComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<BorgDisguiseComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    /// <summary>
    /// Toggles the disguise.
    /// </summary>
    private void OnDisguiseToggle(Entity<BorgDisguiseComponent> ent, ref BorgDisguiseToggleActionEvent args)
    {
        if (args.Handled)
            return;
        ent.Comp.Disguised = !ent.Comp.Disguised;
        Dirty(ent);
        args.Handled = true;

        _actions.SetToggled(ent.Comp.ActionEntity, ent.Comp.Disguised);
        UpdateAppearance(ent);
    }

    /// <summary>
    /// Disables the disguise.
    /// </summary>
    private void DisableDisguise(Entity<BorgDisguiseComponent> ent)
    {
        ent.Comp.Disguised = false;
        _actions.SetToggled(ent.Comp.ActionEntity, false);
        Dirty(ent);

        UpdateAppearance(ent);
    }

    /// <summary>
    /// Disables the disguise if the borg is no longer powered.
    /// </summary>
    private void OnToggled(Entity<BorgDisguiseComponent> ent, ref ItemToggledEvent args)
    {
        if (!args.Activated)
            DisableDisguise(ent);
    }

    /// <summary>
    /// Disables the disguise if the borg is no longer alive.
    /// </summary>
    private void OnMobStateChanged(Entity<BorgDisguiseComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            DisableDisguise(ent);
    }

    /// <summary>
    /// Updates the appearance data of the entity.
    /// </summary>
    private void UpdateAppearance(Entity<BorgDisguiseComponent> ent)
    {
        if (TryPrototype(ent.Owner, out var entityPrototype)
            && entityPrototype.TryComp(_lightName, out PointLightComponent? lightPrototype))
        {
            _light.SetColor(ent.Owner,
                ent.Comp.Disguised
                    ? ent.Comp.DisguisedLightColor
                    : lightPrototype.Color);
        }

        UpdateSharedAppearance(ent);
    }
}
