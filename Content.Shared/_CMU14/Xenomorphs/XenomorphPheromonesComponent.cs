using Content.Shared._Dumont.Xenomorphs;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphPheromonesComponent : Component
{
    [DataField, AutoNetworkedField] public int PlasmaCost = 35;
    [DataField, AutoNetworkedField] public FixedPoint2 PlasmaUpkeep = 2.5;
    [DataField, AutoNetworkedField] public float Range = 8;
    [DataField, AutoNetworkedField] public float Multiplier = 1;
    [DataField, AutoNetworkedField] public string? PheroSuffix;
    [DataField, AutoNetworkedField] public string ActiveMode = "";
}

public sealed partial class XenomorphPheromonesActionEvent : InstantActionEvent;
