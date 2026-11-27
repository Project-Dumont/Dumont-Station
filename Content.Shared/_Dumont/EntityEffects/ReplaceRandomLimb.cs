using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.EntityEffects;

public sealed partial class ReplaceRandomLimb : EventEntityEffect<ReplaceRandomLimb>
{
    [DataField(required: true)]
    public List<LimbReplacement> Limbs = new();

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}

[DataDefinition]
public sealed partial class LimbReplacement
{
    [DataField(required: true)]
    public EntProtoId Part;

    [DataField]
    public EntProtoId? Child;
}
