using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[Serializable, NetSerializable]
public enum XenomorphConstructionUiKey : byte { Key }

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphConstructionComponent : Component
{
    [DataField, AutoNetworkedField] public EntProtoId Choice = "WallResin";
    public Dictionary<string, List<EntityUid>> Built = new();
}

public sealed partial class XenomorphChooseResinActionEvent : InstantActionEvent;
public sealed partial class XenomorphSecreteResinActionEvent : WorldTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphConstructionDoAfterEvent : SimpleDoAfterEvent
{
    public EntProtoId Choice;
    public NetCoordinates Coordinates;
    public NetEntity? Effect;
}
