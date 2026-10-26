using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphFortifyComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Fortified;

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField]
    public int Armor = 30;

    [DataField]
    public int FrontalArmor = 5;

    [DataField]
    public int ExplosionArmor = 60;
}

public sealed partial class XenomorphFortifyActionEvent : InstantActionEvent;
