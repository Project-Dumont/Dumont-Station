using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphOvipositorComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Attached;

    [DataField]
    public int PlasmaCost = 500;

    [DataField]
    public TimeSpan AttachDelay = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan DetachDelay = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan EggDelay = TimeSpan.FromSeconds(30);

    [DataField]
    public EntProtoId Egg = "DumontXenomorphEggItem";

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextEgg;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan ReadyAt;
}

public sealed partial class XenomorphOvipositorActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphOvipositorDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public bool Attach;
}
