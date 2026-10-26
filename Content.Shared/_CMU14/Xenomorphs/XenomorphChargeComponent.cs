using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[Serializable, NetSerializable]
public enum XenomorphChargeMode : byte
{
    Plow,
    Headbutt,
    Gore,
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphChargeComponent : Component
{
    [DataField, AutoNetworkedField] public bool Active;
    [DataField, AutoNetworkedField] public int Stage;
    [DataField, AutoNetworkedField] public XenomorphChargeMode Mode;
    [DataField] public int MaxStage = 8;
    [DataField] public int MinimumSteps = 3;
    [DataField] public int PlasmaPerStep = 6;
    [DataField] public float SpeedPerStage = 0.16f;
    [DataField] public float StepDistance = 1;
    [DataField] public TimeSpan MoveGrace = TimeSpan.FromSeconds(0.5);
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan LastMovedAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextRoar;
    public MoveButtons Direction;
    public Angle RelativeRotation;
    public EntityUid? MovementParent;
    public float Distance;
    public int Steps;
    public EntityUid? Action;
}

public sealed partial class XenomorphChargeActionEvent : InstantActionEvent;

public sealed partial class XenomorphChargeModeActionEvent : InstantActionEvent
{
    [DataField] public XenomorphChargeMode Mode;
}
