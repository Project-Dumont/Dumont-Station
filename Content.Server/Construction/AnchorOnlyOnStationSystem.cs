// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Station.Systems;
using Content.Shared.Construction.Components;
using Content.Shared.Popups;

namespace Content.Server.Construction;

public sealed class AnchorOnlyOnStationSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnchorOnlyOnStationComponent, AnchorAttemptEvent>(OnAnchorAttempt);
    }

    private void OnAnchorAttempt(Entity<AnchorOnlyOnStationComponent> ent, ref AnchorAttemptEvent args)
    {
        if (_station.GetOwningStation(ent) != null)
            return;

        _popup.PopupEntity(Loc.GetString(ent.Comp.PopupMessageAnchorFail), ent, args.User);
        args.Cancel();
    }
}
