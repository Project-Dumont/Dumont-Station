using Content.Shared._Dumont.Xenomorphs;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphAcidPillarComponent : Component
{
    [DataField] public float Range = 5;
    [DataField] public TimeSpan CheckEvery = TimeSpan.FromSeconds(1);
    [DataField] public TimeSpan Cooldown = TimeSpan.FromSeconds(1);
    [DataField] public TimeSpan AcidSpreadDelay = TimeSpan.FromSeconds(.3);
    public TimeSpan Next;
    [DataField, AutoNetworkedField] public TimeSpan FiringUntil;
}
