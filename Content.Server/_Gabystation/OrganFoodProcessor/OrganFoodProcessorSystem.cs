using Content.Server.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Body.Organ;
using Content.Shared.Chemistry.Components.SolutionManager;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Shared.Jittering;

namespace Content.Server._Gabystation.OrganFoodProcessor
{
    public sealed class OrganFoodProcessorSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly SharedJitteringSystem _jittering = default!;

        public const string DefaultSolutionName = "stomach";
        public override void Initialize()
        {
        }

        public bool TrySynthProcessingFood(EntityUid stomach_uid, StomachComponent stomach, OrganComponent organ, SolutionContainerManagerComponent sol)
        {
            if (!TryComp<OrganFoodProcessorComponent>(stomach_uid, out var foodProcessor))
            {
                return false;
            }
            if (!_solutionContainerSystem.ResolveSolution((stomach_uid, sol), DefaultSolutionName, ref stomach.Solution, out var stomachSolution))
            {
                return false;
            }

            if (stomach.ReagentDeltas.Count == 0)
            {
                return false;
            }
            if (organ.Body is null)
            {
                return false;
            }
            EntityUid body = organ.Body.Value;

            var queue = new RemQueue<StomachComponent.ReagentDelta>();
            foreach (var delta in stomach.ReagentDeltas)
            {
                if (stomachSolution.TryGetReagent(delta.ReagentQuantity.Reagent, out var reagent))
                {
                    if (reagent.Quantity > delta.ReagentQuantity.Quantity)
                        reagent = new(reagent.Reagent, delta.ReagentQuantity.Quantity);

                    stomachSolution.RemoveReagent(reagent);
                }

                queue.Add(delta);
            }
            foreach (var item in queue)
            {
                stomach.ReagentDeltas.Remove(item);
            }
            AudioParams audioParams = AudioParams.Default;
            audioParams.Volume = 0.5f;

            _audio.PlayPvs(foodProcessor.ProcessingSound, body, audioParams);
            TimeSpan duration = TimeSpan.FromSeconds(2.5f);
            _jittering.DoJitter(body, duration, true, frequency: 300f);



            return true;

        }
    }
}
