using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Salvage;

[Prototype]
public sealed partial class SalvageMissionObjectiveHandlerPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [DataField]
    public BaseSalvageMissionObjectiveHandler? Handler;
}