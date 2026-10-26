using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphVampireComponent : Component
{
    [DataField, AutoNetworkedField] public bool Headbiting;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan FlurryHealAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan BiteHealAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextRoar;
    public int FlurryHeal;
    public int BiteHeal;
    public EntityUid? HeadbiteAction;
}

public sealed partial class XenomorphFlurryActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphTailJabActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphHeadbiteActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphHeadbiteDoAfterEvent : SimpleDoAfterEvent;
