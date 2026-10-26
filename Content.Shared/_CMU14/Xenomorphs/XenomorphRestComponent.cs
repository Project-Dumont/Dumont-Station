using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphRestComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Resting;

    [DataField, AutoNetworkedField]
    public EntityUid? RestAction;
}

public sealed partial class XenomorphRestActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public enum XenomorphVisuals : byte
{
    Resting,
    Ovipositor,
    Fortified,
    Invisible,
    Crest,
}
