// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Temperature.Components;
using Content.Shared.Temperature;
using Content.Shared._Trauma.Temperature;

namespace Content.Server._Trauma.Temperature;

// Dumont - lives on server since our TemperatureComponent is server only
public sealed partial class BlackBodySystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private EntityQuery<TemperatureComponent> _tempQuery = default!;

    [SubscribeLocalEvent]
    private void OnStartup(Entity<BlackBodyComponent> ent, ref ComponentStartup args)
    {
        EnsureComp<AppearanceComponent>(ent); // fails tests if you forget this in your prototype
        if (_tempQuery.TryComp(ent, out var temp))
            _appearance.SetData(ent.Owner, BlackBodyVisuals.Temperature, temp.CurrentTemperature);
    }

    [SubscribeLocalEvent]
    private void OnTemperatureChange(Entity<BlackBodyComponent> ent, ref OnTemperatureChangeEvent args)
    {
        _appearance.SetData(ent.Owner, BlackBodyVisuals.Temperature, args.CurrentTemperature);
    }
}
