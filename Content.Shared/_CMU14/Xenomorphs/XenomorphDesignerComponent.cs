using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[Serializable, NetSerializable]
public enum XenomorphDesignType : byte { Optimized, Flexible, Construct }

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphDesignerComponent : Component
{
    [DataField, AutoNetworkedField] public bool Door;
    [DataField, AutoNetworkedField] public int Count;
    [DataField] public int Limit = 36;
    public readonly List<EntityUid> Nodes = new();
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphDesignNodeComponent : Component
{
    [DataField, AutoNetworkedField] public XenomorphDesignType Kind;
    [DataField, AutoNetworkedField] public bool Door;
    [DataField, AutoNetworkedField] public EntityUid? Designer;
    [DataField, AutoNetworkedField] public EntityUid? Weed;
}

public sealed partial class XenomorphDesignActionEvent : WorldTargetActionEvent
{
    [DataField] public XenomorphDesignType Kind;
}
public sealed partial class XenomorphDesignToggleActionEvent : InstantActionEvent;
public sealed partial class XenomorphDesignRemoveActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphDesignThickenActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphDesignSurgeActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphDesignDoAfterEvent : SimpleDoAfterEvent
{
    public NetCoordinates Coordinates;
    public XenomorphDesignType Kind;
    public bool Door;
}
[Serializable, NetSerializable]
public sealed partial class XenomorphDesignThickenDoAfterEvent : SimpleDoAfterEvent;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphResinBuildVisualComponent : Component
{
    [DataField, AutoNetworkedField] public float Duration = 1;
    [DataField(customTypeSerializer: typeof(Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan End;
}
