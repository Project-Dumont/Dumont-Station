using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphRecoveryNodeComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 HealAmount = 25;

    [DataField, AutoNetworkedField]
    public float HealRange = 1.5F;

    [DataField, AutoNetworkedField]
    public TimeSpan HealCooldown = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public TimeSpan NextHealAt;

    [DataField]
    public DoAfterId? HealDoAfter;
}
