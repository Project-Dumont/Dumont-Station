using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphValkyrieComponent : Component
{
    [DataField, AutoNetworkedField] public int Fury;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextGain;
    public EntityUid? RetrieveAction;
    public readonly List<EntityUid> Telegraphs = new();
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphTantrumComponent : Component
{
    [DataField, AutoNetworkedField] public int Armor = 15;
    [DataField, AutoNetworkedField] public float Speed = 1;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan Until;
}

public sealed partial class XenomorphTailFountainActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphTantrumActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphHighGallopActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphFightOrFlightActionEvent : InstantActionEvent;
public sealed partial class XenomorphRetrieveActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphFuryAlertEvent : BaseAlertEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphRetrieveDoAfterEvent : SimpleDoAfterEvent;
