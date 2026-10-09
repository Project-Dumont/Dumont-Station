// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Goobstation.Common.Religion;
using Content.Goobstation.Common.Singularity;
using Content.Shared.Examine;
using Content.Trauma.Common.Heretic;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems.PathSpecific.Lock;

public sealed partial class LabyrinthHandbookSystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;
    [Dependency] private SharedHereticSystem _heretic = default!;
    [Dependency] private EntityQuery<LabyrinthWallComponent> _wallQuery = default!;

    [SubscribeLocalEvent]
    private void OnThrow(ref ContainmentFieldThrowEvent args)
    {
        if (!_wallQuery.HasComp(args.Field))
            return;

        if (_heretic.IsHereticOrGhoul(args.Entity))
        {
            args.Cancelled = true;
            return;
        }

        var ev = new BeforeCastTouchSpellEvent(args.Entity, false);
        RaiseLocalEvent(args.Entity, ev, true);
        args.Cancelled = ev.Cancelled;
    }

    [SubscribeLocalEvent]
    private void OnBeforeHolosign(Entity<LabyrinthHandbookComponent> ent, ref BeforeHolosignUsedEvent args)
    {
        args.Handled = true;

        if (!_heretic.IsHereticOrGhoul(args.User) || !_examine.InRangeUnOccluded(args.User, args.ClickLocation))
            args.Cancelled = true;
    }
}
