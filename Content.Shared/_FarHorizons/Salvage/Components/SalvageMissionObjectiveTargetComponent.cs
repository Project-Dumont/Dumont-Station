using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Salvage.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SalvageMissionObjectiveTargetComponent : Component
{
    public ProtoId<SalvageMissionObjectivePrototype>? OwnedBy = null;
}