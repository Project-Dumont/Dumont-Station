using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphReaperComponent : Component
{
    [DataField, AutoNetworkedField] public int Flesh;
    [DataField, AutoNetworkedField] public int MaxFlesh = 1000;
    [DataField] public int PassiveCap = 300;
    [DataField] public int PassiveGain = 2;
    [DataField] public int MeleeGain = 5;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextGain;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphCarrionMantleComponent : Component
{
    [DataField, AutoNetworkedField] public bool Active = true;
    [DataField, AutoNetworkedField] public float Shield = 200;
    [DataField] public int Armor = 15;
    [DataField] public float SpeedMultiplier = 0.9f;
    [DataField] public float ShieldDecay = 70;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ExpiresAt;
    public EntityUid? Caster;
    public EntityUid? Effect;
}

public sealed partial class XenomorphFleshHarvestActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphRaptureActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphFleshBloomActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphRedGasActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphCarrionMantleActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphFleshAlertEvent : BaseAlertEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphFleshHarvestDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphFleshBloomDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public NetCoordinates Coordinates;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphRedGasDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public NetCoordinates[] Path = Array.Empty<NetCoordinates>();
    [DataField] public int Step;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphCarrionShieldVisualComponent : Component;

[Serializable, NetSerializable]
public enum XenomorphReaperVisuals : byte { ShieldFraction }
