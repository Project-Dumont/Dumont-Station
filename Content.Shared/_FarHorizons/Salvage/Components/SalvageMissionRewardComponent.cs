using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Salvage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SalvageMissionRewardComponent : Component
{
    [ViewVariables]
    public bool MissionCompleted = false;
    [ViewVariables]
    public int Bonuses = 0;
    [ViewVariables]
    public int MaxBonuses = 0;
    [ViewVariables]
    public int TotalReward = 0;
    [ViewVariables]
    public ProtoId<SalvageMissionObjectivePrototype> parentObjective = "";

    [ViewVariables] public float CashMultiplier;
}