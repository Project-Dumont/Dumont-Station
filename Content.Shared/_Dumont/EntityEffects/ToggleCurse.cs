using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.EntityEffects;

/// <summary>
/// Cures the target if cursed, curses it otherwise. Purges the reagent so it doesn't flip back next tick.
/// </summary>
public sealed partial class ToggleCurse : EventEntityEffect<ToggleCurse>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-toggle-curse", ("chance", Probability));
}
