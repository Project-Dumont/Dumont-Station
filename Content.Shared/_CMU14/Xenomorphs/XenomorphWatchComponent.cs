using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphWatchComponent : Component
{
    [DataField, AutoNetworkedField] public EntityUid? Watching;
}

public sealed partial class XenomorphWatchActionEvent : InstantActionEvent;
