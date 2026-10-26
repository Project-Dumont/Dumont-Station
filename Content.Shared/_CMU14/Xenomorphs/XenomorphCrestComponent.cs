using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphCrestComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Lowered;

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField]
    public int Armor = 5;

    [DataField]
    public float SpeedMultiplier = 0.7f;
}

public sealed partial class XenomorphCrestActionEvent : InstantActionEvent;
