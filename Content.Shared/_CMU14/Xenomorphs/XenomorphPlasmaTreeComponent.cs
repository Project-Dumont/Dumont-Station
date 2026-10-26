using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphPlasmaTreeComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaAmount = 75;

    [DataField, AutoNetworkedField]
    public float PlasmaRange = 1.5F;

    [DataField, AutoNetworkedField]
    public TimeSpan PlasmaCooldown = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public TimeSpan NextPlasmaAt;

    [DataField]
    public DoAfterId? PlasmaDoAfter;
}
