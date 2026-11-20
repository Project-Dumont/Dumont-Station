// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Physics;
// Dumont end

namespace Content.Trauma.Shared.Physics.ComplexJoint;

[ByRefEvent]
public readonly record struct ComplexJointUpdateEvent(EntityUid Uid, Dictionary<string, HashSet<EntityUid>> UpdatedIds);

[ByRefEvent]
public record struct ComplexJointCollisionEvent(
    EntityUid Origin,
    RayCastResults Hit,
    EntityUid Target,
    HereticComplexJointVisualsData Data,
    bool BlockNextCollisions = false);

[ByRefEvent]
public record struct BeforeContinuousBeamDamagedEvent(EntityUid Uid, EntityUid Target, bool Cancelled = false);

[ByRefEvent]
public readonly record struct AfterContinuousBeamDamagedEvent(EntityUid Uid, EntityUid Target);

[ByRefEvent]
public readonly record struct ContinuousBeamStoppedFiringEvent;

[ByRefEvent]
public record struct BeforeContinuousBeamDamageTickEvent(
    Entity<ContinuousBeamGunComponent, HereticComplexJointVisualsComponent> Ent,
    bool Cancelled = false);
