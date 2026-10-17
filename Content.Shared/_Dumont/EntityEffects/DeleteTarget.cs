using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.EntityEffects;

public sealed partial class DeleteTarget : EventEntityEffect<DeleteTarget>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
