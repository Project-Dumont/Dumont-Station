using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.EntityEffects;
public sealed partial class StationAddParallax : EventEntityEffect<StationAddParallax>
{
    [DataField(required: true)]
    public string Parallax = string.Empty;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
