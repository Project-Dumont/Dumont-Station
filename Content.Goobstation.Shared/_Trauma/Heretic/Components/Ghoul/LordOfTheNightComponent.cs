// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Whitelist;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Ghoul;

[RegisterComponent, NetworkedComponent]
public sealed partial class LordOfTheNightComponent : Component
{

    /// <summary>
    /// Set to true when flesh heretic mind gets added.
    /// Used to apply FleshHereticMindComponent SustainedDamage to the worm
    /// </summary>
    [DataField]
    public bool HereticInitialized;

    [DataField]
    public EntityWhitelist ArmWhitelist = new()
    {
        // Dumont start
        Tags = new() { "HereticRitualArm" },
        // Dumont end
    };

    [DataField]
    public EntityWhitelist UnanchorWhitelist = new()
    {
        Components = new[] { "Door", "Destructible", "Wall" },
        Tags = new() { "Window", "Structure" },
    };

    [DataField]
    public EntityWhitelist PushBlacklist = new()
    {
        Tags = new() { "FleshWormSegment" },
    };

    [DataField]
    public FixedPoint2 HealPerArm = 250;

    [DataField]
    public FixedPoint2 HealthPerSegment = 250;

    [DataField]
    public float ArmDelimbChance = 0.25f;

    [DataField]
    // Dumont start
    public Content.Shared.Body.Part.BodyPartSymmetry ArmLeft = Content.Shared.Body.Part.BodyPartSymmetry.Left;
    // Dumont end

    [DataField]
    // Dumont start
    public Content.Shared.Body.Part.BodyPartSymmetry ArmRight = Content.Shared.Body.Part.BodyPartSymmetry.Right;
    // Dumont end

    [DataField(required: true)]
    public EntityEffect[] MadnessEffects;

    [DataField]
    public float MadnessRange = 8f;

    [DataField]
    public EntProtoId TransformAction = "ActionHereticFleshTransform";

    [DataField]
    public float ForceMultiplier = 0.1f;
}
