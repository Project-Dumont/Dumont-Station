// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using System.Linq;
// Dumont end

namespace Content.Trauma.Shared.Physics.ComplexJoint;

public abstract partial class SharedComplexJointVisualsSystem : EntitySystem
{
    public void ClearBeamJoints(Entity<HereticComplexJointVisualsComponent?> ent, string excludedId, EntityUid? target = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        TryGetNetEntity(target, out var netTarget);

        ent.Comp.Data = ent.Comp.Data.Where(x => netTarget is { } t && x.Key != t || x.Value.Id != excludedId)
            .ToDictionary();

        if (ent.Comp.Data.Count == 0)
            RemComp(ent.Owner, ent.Comp);
        else
            Dirty(ent.Owner, ent.Comp);
    }

    /// <summary>
    /// Adds complex joint comp on uidB and links it to uidA as data dictionary key
    /// </summary>
    public void CreateJoint(EntityUid uidA, EntityUid uidB, HereticComplexJointVisualsData data)
    {
        var beam = EnsureComp<HereticComplexJointVisualsComponent>(uidB);
        beam.Data[GetNetEntity(uidA)] = data;
        Dirty(uidB, beam);
    }

    public Dictionary<NetEntity, HereticComplexJointVisualsData> GetJointData(HereticComplexJointVisualsComponent joint,
        string id)
    {
        return joint.Data.Where(x => x.Value.Id == id).ToDictionary();
    }
}
