using Content.Shared._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class XenomorphLongRangeSightComponent : Component
{
    [DataField, AutoNetworkedField] public bool Enabled;
    [DataField, AutoNetworkedField] public Vector2 Offset;
    [DataField, AutoNetworkedField] public float OffsetLength = 3;
    [DataField, AutoNetworkedField] public float Speed = 0.8f;
    [DataField, AutoNetworkedField] public Vector2 Zoom = Vector2.One;
    [DataField] public TimeSpan PrepareDelay = TimeSpan.FromSeconds(0.5);
    public bool Preparing;
}

public sealed partial class XenomorphLongRangeSightActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphLongRangeSightDoAfterEvent : SimpleDoAfterEvent;
