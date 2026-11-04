using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.EntityEffects;

public sealed partial class ReplaceOrgan : EventEntityEffect<ReplaceOrgan>
{
    [DataField(required: true)]
    public EntProtoId Organ;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
