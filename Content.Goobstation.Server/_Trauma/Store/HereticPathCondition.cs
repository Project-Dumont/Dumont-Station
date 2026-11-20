// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Store;
using Content.Trauma.Server.Heretic.Systems;
using Content.Trauma.Shared.Heretic.Components;
// Dumont end

namespace Content.Trauma.Server.Heretic.Store;

public sealed partial class HereticPathCondition : ListingCondition
{
    [DataField]
    public HashSet<HereticPath>? Whitelist;

    [DataField]
    public HashSet<HereticPath>? Blacklist;

    [DataField]
    public int Stage;

    [DataField]
    public bool RequiresCanAscend;

    [DataField]
    public int MinPassiveLevel;

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;
        var hereticSys = ent.System<HereticSystem>();

        if (!hereticSys.TryGetHereticComponent(args.Buyer, out var hereticComp, out _) &&
            !ent.TryGetComponent(args.Buyer, out hereticComp))
            return false;

        if (hereticComp.PassiveLevel < MinPassiveLevel)
            return false;

        if (RequiresCanAscend && !hereticComp.CanAscend)
            return false;

        if (Stage > hereticComp.PathStage)
            return false;

        if (Whitelist != null)
        {
            foreach (var white in Whitelist)
            {
                if (hereticComp.CurrentPath == white)
                    return true;
            }

            return false;
        }

        if (Blacklist == null)
            return true;

        foreach (var black in Blacklist)
        {
            if (hereticComp.CurrentPath == black)
                return false;
        }

        return true;
    }
}
