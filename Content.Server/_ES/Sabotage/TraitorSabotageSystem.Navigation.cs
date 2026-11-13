// SPDX-FileCopyrightText: 2025-2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Shared.Shuttles.Components;

namespace Content.Server._ES.Sabotage;

public sealed partial class TraitorSabotageSystem
{
    [Dependency] private HeavyGravitySystem _heavyGravity = default!;
    [Dependency] private SpaceTurbulenceSystem _turbulence = default!;

    partial void InitializeNavigation()
    {
        SubscribeLocalEvent<Content.Server.Gravity.GravityGeneratorComponent, SabotageEffectEvent>(OnGravitySabotage);
        SubscribeLocalEvent<Content.Server.Gravity.GravityGeneratorComponent, SabotageRepairedEvent>(OnGravityRepaired);
        SubscribeLocalEvent<RadarConsoleComponent, SabotageEffectEvent>(OnNavigationSabotage);
    }

    private void OnGravitySabotage(Entity<Content.Server.Gravity.GravityGeneratorComponent> ent, ref SabotageEffectEvent args)
    {
        if (args.Handled || !TryComp<SabotageTargetComponent>(ent, out var sabotage))
            return;

        args.Handled = _heavyGravity.TryStart(ent);
        if (args.Handled)
            sabotage.Sabotaged = true;
    }

    private void OnGravityRepaired(Entity<Content.Server.Gravity.GravityGeneratorComponent> ent, ref SabotageRepairedEvent args)
    {
        _heavyGravity.Stop(ent);
    }

    private void OnNavigationSabotage(Entity<RadarConsoleComponent> ent, ref SabotageEffectEvent args)
    {
        if (args.Handled || !HasComp<SabotageTargetComponent>(ent)
            || _station.GetOwningStation(ent) is not {} station)
            return;

        args.Handled = _turbulence.TryStart(station);
    }
}
