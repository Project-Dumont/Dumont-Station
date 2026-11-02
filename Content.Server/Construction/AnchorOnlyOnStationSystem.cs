// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Station.Systems;
using Content.Shared.Construction.Components;
using Content.Shared.Popups;

namespace Content.Server.Construction;

public sealed partial class AnchorOnlyOnStationSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnchorOnlyOnStationComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AnchorOnlyOnStationComponent, AnchorAttemptEvent>(OnAnchorAttempt);
    }

    private void OnMapInit(Entity<AnchorOnlyOnStationComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.AnchorOnSpawn || _station.GetOwningStation(ent) == null)
            return;

        var xform = Transform(ent);
        if (!xform.Anchored)
            _transform.AnchorEntity((ent.Owner, xform));
    }

    private void OnAnchorAttempt(Entity<AnchorOnlyOnStationComponent> ent, ref AnchorAttemptEvent args)
    {
        if (_station.GetOwningStation(ent) != null)
            return;

        _popup.PopupEntity(Loc.GetString(ent.Comp.PopupMessageAnchorFail), ent, args.User);
        args.Cancel();
    }
}
