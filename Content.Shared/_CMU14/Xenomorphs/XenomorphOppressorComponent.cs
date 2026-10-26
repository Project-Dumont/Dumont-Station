using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class XenomorphOppressorComponent : Component
{
    [DataField, AutoNetworkedField] public bool Preparing;
    [DataField, AutoNetworkedField] public bool RootedPreparing;
    public Box2Rotated? LashArea;
    public EntityUid? LashGrid;
    public EntityUid? PreparingAction;
    public readonly List<EntityUid> Telegraphs = new();
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class XenomorphTailTetherComponent : Component
{
    [DataField, AutoNetworkedField] public EntityUid Source;
    public bool Seized;
}

public sealed partial class XenomorphTailSeizeActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphAbductActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphTailLashActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphDislocateActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphOppressorDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public bool Lash;
}
