// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Disease.Components;
using Content.Server.Actions;
using Content.Shared._Orion.ToggleableHUD;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Overlays;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Robust.Shared.Prototypes;

namespace Content.Server._Orion.ToggleableHUD;

/// <summary>
///     System for toggling HUDs.
/// </summary>
public sealed class ToggleableHudSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleableHUDComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ToggleableHUDComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ToggleableHUDComponent, ToggleActionEvent>(OnToggle);
    }

    private void OnMapInit(Entity<ToggleableHUDComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        _actions.AddAction(uid, ref comp.ActionEntity, comp.Action, uid);
    }

    private void OnShutdown(Entity<ToggleableHUDComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    private void OnToggle(Entity<ToggleableHUDComponent> ent, ref ToggleActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryToggle(ent);
    }

    public bool TryToggle(Entity<ToggleableHUDComponent> ent)
    {
        var actionToggle = !ent.Comp.IsToggled;

        var popupKey = actionToggle ? ent.Comp.PopupToggleOn : ent.Comp.PopupToggleOff;
        var popupMessage = Loc.GetString(popupKey);

        // 这是个好办法，我发誓 😱
        if (actionToggle)
        {
            EnsureComp<ShowJobIconsComponent>(ent);

            // HealthBars should have a list of damage containers
            var healthBars = EnsureComp<ShowHealthBarsComponent>(ent);
            healthBars.DamageContainers = new List<ProtoId<DamageContainerPrototype>>(ent.Comp.DamageContainers);
            Dirty(ent, healthBars);
            EnsureComp<ShowHealthIconsComponent>(ent);

            if (ent.Comp.IsAdmin)
            {
                EnsureComp<ShowMindShieldIconsComponent>(ent);
                EnsureComp<ShowCriminalRecordIconsComponent>(ent);

                EnsureComp<ShowDiseaseIconsComponent>(ent);

                EnsureComp<ShowSyndicateIconsComponent>(ent);

                EnsureComp<ShowHungerIconsComponent>(ent);
                EnsureComp<ShowThirstIconsComponent>(ent);
            }
        }
        else
        {
            RemComp<ShowJobIconsComponent>(ent);
            RemComp<ShowHealthBarsComponent>(ent);
            RemComp<ShowHealthIconsComponent>(ent);

            if (ent.Comp.IsAdmin)
            {
                RemComp<ShowMindShieldIconsComponent>(ent);
                RemComp<ShowCriminalRecordIconsComponent>(ent);

                RemComp<ShowDiseaseIconsComponent>(ent);

                RemComp<ShowSyndicateIconsComponent>(ent);

                RemComp<ShowHungerIconsComponent>(ent);
                RemComp<ShowThirstIconsComponent>(ent);
            }
        }

        _popup.PopupEntity(popupMessage, ent, ent);

        ent.Comp.IsToggled = actionToggle;
        Dirty(ent);

        return true;
    }
}
