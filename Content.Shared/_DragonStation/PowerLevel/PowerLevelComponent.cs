using Robust.Shared.GameStates;

namespace Content.Shared._DragonStation.PowerLevel;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PowerLevelComponent : Component
{
    [DataField, AutoNetworkedField]
    public int CurrentPowerLevel = 100;

    [ViewVariables(VVAccess.ReadOnly)]
    public float LastOffenseScore;

    [ViewVariables(VVAccess.ReadOnly)]
    public float LastMobilityScore;

    [ViewVariables(VVAccess.ReadOnly)]
    public float LastDefenseScore = 1f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float LastDurabilityScore;
}
