// SPDX-FileCopyrightText: 2025-2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Gravity;
using Content.Shared.Shuttles.Components;

namespace Content.Server._ES.Sabotage;

public sealed partial class TraitorSabotageSystem
{
    [Dependency] private PowerChargeSystem _powerCharge = default!;
    [Dependency] private PowerReceiverSystem _powerReceiver = default!;

    partial void InitializeNavigation()
    {
        SubscribeLocalEvent<GravityGeneratorComponent, SabotageEffectEvent>(OnGravitySabotage);
        SubscribeLocalEvent<RadarConsoleComponent, SabotageEffectEvent>(OnNavigationSabotage);
        SubscribeLocalEvent<RadarConsoleComponent, SabotageRepairedEvent>(OnNavigationRepaired);
    }

    private void OnGravitySabotage(Entity<GravityGeneratorComponent> ent, ref SabotageEffectEvent args)
    {
        if (args.Handled || !TryComp<PowerChargeComponent>(ent, out var charge))
            return;

        if (ent.Comp.GravityActive)
            _chat.DispatchStationAnnouncement(ent, Loc.GetString("es-sabotage-gravity-announcement"));
        _powerCharge.Discharge(ent, charge);
        args.Handled = true;
    }

    private void OnNavigationSabotage(Entity<RadarConsoleComponent> ent, ref SabotageEffectEvent args)
    {
        if (args.Handled || !TryComp<SabotageTargetComponent>(ent, out var sabotage))
            return;

        // Dumont has local navigation consoles, not ES's station warp drive.
        sabotage.Sabotaged = true;
        _powerReceiver.SetPowerDisabled(ent, true);
        args.Handled = true;
    }

    private void OnNavigationRepaired(Entity<RadarConsoleComponent> ent, ref SabotageRepairedEvent args)
    {
        _powerReceiver.SetPowerDisabled(ent, false);
    }
}
