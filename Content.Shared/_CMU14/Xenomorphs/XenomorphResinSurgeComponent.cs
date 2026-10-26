using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphResinSurgeComponent : Component
{
    [DataField, AutoNetworkedField] public bool Busy;
}

public sealed partial class XenomorphResinSurgeActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphPlantHardyWeedsActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphResinSurgeDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public NetCoordinates Coordinates;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphSurgeReinforcableComponent : Component;
