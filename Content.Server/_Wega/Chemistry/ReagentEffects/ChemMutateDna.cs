using Content.Server.Genetics.System;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using JetBrains.Annotations;

namespace Content.Server.Chemistry.ReagentEffects
{
    [UsedImplicitly]
    public sealed partial class ChemMutateDna : EntityEffect
    {
        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            return Loc.GetString("");
        }

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args is not EntityEffectReagentArgs reagentArgs)
                return;

            var ev = new MutateDnaAttemptEvent();
            args.EntityManager.EventBus.RaiseLocalEvent(reagentArgs.TargetEntity, ev, false);
        }
    }
}
