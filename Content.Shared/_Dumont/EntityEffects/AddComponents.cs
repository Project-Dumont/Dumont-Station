using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.EntityEffects;

public sealed partial class AddComponents : EventEntityEffect<AddComponents>
{
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
