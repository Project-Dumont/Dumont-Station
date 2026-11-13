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
    [Dependency] private SpaceTurbulenceSystem _turbulence = default!;

    partial void InitializeNavigation()
    {
        SubscribeLocalEvent<GravityGeneratorComponent, SabotageEffectEvent>(OnGravitySabotage);
        SubscribeLocalEvent<RadarConsoleComponent, SabotageEffectEvent>(OnNavigationSabotage);
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
        if (args.Handled || _station.GetOwningStation(ent) is not {} station)
            return;

        args.Handled = _turbulence.TryStart(station);
    }
}
