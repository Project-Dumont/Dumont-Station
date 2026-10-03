// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Trauma.Shared.Genetics.Mutations;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects;

public sealed partial class GeneticDisorder : EventEntityEffect<GeneticDisorder>
{
    [DataField]
    public bool Remove;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString(Remove
                ? "entity-effect-guidebook-cure-disorder"
                : "entity-effect-guidebook-cause-disorder",
            ("chance", Probability));
}

public sealed partial class GeneticDisorderEntityEffectSystem : TraumaEntityEffectSystem<MutatableComponent, GeneticDisorder>
{
    [Dependency] private MutationSystem _mutation = default!;

    protected override void Effect(Entity<MutatableComponent> ent, GeneticDisorder effect, EntityEffectBaseArgs args)
    {
        if (effect.Remove)
            _mutation.RemoveRandomDisorder(ent);
        else
            _mutation.AddRandomDisorder(ent);
    }
}
