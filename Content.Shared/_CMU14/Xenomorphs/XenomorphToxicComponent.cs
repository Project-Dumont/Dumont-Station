using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphToxicComponent : Component
{
    [DataField, AutoNetworkedField] public int HitsRemaining;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan SlashUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan SpeedUntil;
    public EntityUid? SlashAction;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphIntoxicatedComponent : Component
{
    [DataField, AutoNetworkedField] public int Stacks;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextTick;
    [DataField, AutoNetworkedField] public bool Resisting;
    public EntityUid? LastSource;
    public bool WasDown;
    public bool ForcedDown;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphDrainSurgeComponent : Component
{
    [DataField, AutoNetworkedField] public int Armor = 15;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan ExpiresAt;
}

public sealed partial class XenomorphToxicSpitActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphToxicSlashActionEvent : InstantActionEvent;
public sealed partial class XenomorphDrainStingActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphWeakCorrodeActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphIntoxicatedResistAlertEvent : BaseAlertEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphIntoxicatedResistDoAfterEvent : SimpleDoAfterEvent;
[Serializable, NetSerializable]
public sealed partial class XenomorphWeakCorrodeDoAfterEvent : SimpleDoAfterEvent;
