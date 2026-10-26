using Content.Shared._Dumont.Xenomorphs;
using Robust.Shared.GameStates;
using Content.Shared.Alert;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphArmorComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Armor;

    [DataField, AutoNetworkedField]
    public int FrontalArmor;

    [DataField, AutoNetworkedField]
    public int SideArmor;

    [DataField, AutoNetworkedField]
    public int ExplosionArmor;

    [DataField, AutoNetworkedField]
    public int DamageMultiplier = 4;

    [DataField, AutoNetworkedField]
    public bool FlatReduction = true;
}

public sealed partial class XenomorphArmorAlertEvent : BaseAlertEvent;
