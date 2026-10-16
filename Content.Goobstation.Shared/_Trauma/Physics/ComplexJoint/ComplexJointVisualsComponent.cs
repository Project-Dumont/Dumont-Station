// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Physics;
// Dumont end

namespace Content.Trauma.Shared.Physics.ComplexJoint;

/// <summary>
/// Works like JointVisualsComponent, but supports multiple targets and more customization.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HereticComplexJointVisualsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<NetEntity, HereticComplexJointVisualsData> Data = new(); // Target -> Data (no more than 1 beam per target)
}

[Serializable, NetSerializable, DataRecord]
public sealed partial class HereticComplexJointVisualsData(
    string id,
    SpriteSpecifier sprite,
    float? maxRange)
{
    public HereticComplexJointVisualsData() : this(string.Empty, SpriteSpecifier.Invalid, null) { }

    public SpriteSpecifier? StartSprite;

    public SpriteSpecifier? EndSprite;

    public SpriteSpecifier Sprite = sprite;

    public Color Color = Color.White;

    public string Id = id;

    public TimeSpan? CreationTime;

    public Vector2 Scale = Vector2.One;

    public float? MaxRange = maxRange;

    #region Collision

    public bool ShouldCollide = true;

    public bool CollisionIgnoreTarget = true;

    public bool CollisionIgnoreParent;

    public bool ReturnOnFirstHit;

    public CollisionGroup CollisionMask = CollisionGroup.Opaque;

    #endregion

    /// <summary>
    /// Will this render/calculate collisions from origin to target (default) or from target to origin
    /// </summary>
    public bool ReverseBeam;
}
