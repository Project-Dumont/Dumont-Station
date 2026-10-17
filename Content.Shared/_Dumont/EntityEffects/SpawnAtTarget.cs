using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.EntityEffects;

public sealed partial class SpawnAtTarget : EventEntityEffect<SpawnAtTarget>
{
    [DataField(required: true)]
    public EntProtoId Entity;

    [DataField]
    public int Number = 1;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
