// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;

[RegisterComponent]
public sealed partial class LeechingWalkComponent : Component
{
    public override bool SessionSpecific => true;

    [DataField]
    public FixedPoint2 BoneHeal = -5;

    [DataField]
    public float StaminaHeal = 5f;

    [DataField]
    public float ChemPurgeRate = 3f;

    [DataField]
    public ProtoId<ReagentPrototype>[] ExcludedReagents =
        ["EldritchEssence", "CrucibleSoul", "DuskAndDawn", "WoundedSoldier", "NewbornEther"];

    [DataField]
    public FixedPoint2 BloodHeal = 5f;

    [DataField]
    public TimeSpan StunReduction = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public float TargetTemperature = 310f;

    [DataField]
    public List<EntProtoId> RemovedStatusEffects = new()
    {
        "StatusEffectForcedSleeping",
        "StatusEffectDrowsiness",
        "StatusEffectSeeingRainbow",
        "StatusEffectBlurryVision",
        "StatusEffectBlindness"
    };
}
