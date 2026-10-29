// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
// Dumont end

namespace Content.Server.Store.Conditions;

/// <summary>
/// Allows a store entry to be filtered out based on the user's job.
/// Supports both blacklists and whitelists
/// </summary>
public sealed partial class BuyerDepartmentCondition : ListingCondition
{
    /// <summary>
    /// A whitelist of department prototypes that can purchase this listing. Only one needs to be found.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>>? Whitelist;

    /// <summary>
    /// A blacklist of department prototypes that can purchase this listing. Only one needs to be found.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>>? Blacklist;

    public override bool Condition(ListingConditionArgs args)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

        var ent = args.EntityManager;

        var mind = args.Buyer; // Goob start

        if (!ent.TryGetComponent<MindComponent>(args.Buyer, out var _)
        && !ent.System<SharedMindSystem>().TryGetMind(args.Buyer, out mind, out _))
            return true;

        var jobs = ent.System<SharedJobSystem>();
        jobs.MindTryGetJob(mind, out var job); //Goob end

        if (Blacklist != null && job != null)
        {
            foreach (var department in prototypeManager.EnumeratePrototypes<DepartmentPrototype>())
            {
                if (department.Roles.Contains(job.ID) && Blacklist.Contains(department.ID))
                    return false;
            }
        }

        if (Whitelist != null)
        {
            var found = false;

            if (job != null)
            {
                foreach (var department in prototypeManager.EnumeratePrototypes<DepartmentPrototype>())
                {
                    if (department.Roles.Contains(job.ID) && Whitelist.Contains(department.ID))
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
                return false;
        }

        return true;
    }
}
