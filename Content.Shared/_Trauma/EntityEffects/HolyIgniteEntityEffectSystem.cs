using Content.Shared.EntityEffects;
using Content.Trauma.Shared.Chaplain;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects;

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class HolyIgnite : EntityEffect
{
    /// <summary>
    ///     Amount of FireStacks improved.
    /// </summary>
    [DataField(required: true)]
    public float Stacks;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-extinguish-reaction", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var ev = new HolyIgniteEvent
        {
            FireStacksAdjustment = Stacks,
        };
        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ref ev);
    }
}
