// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT

using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Body.Organ;
using Content.Shared.Chemistry.Components.SolutionManager;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Shared.Jittering;

namespace Content.Shared._Gabystation.OrganFoodProcessor;

public sealed class OrganFoodProcessorSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;

    public const string DefaultSolutionName = "stomach";

    public bool TrySynthProcessingFood(EntityUid stomachUid, StomachComponent stomach, OrganComponent organ, SolutionContainerManagerComponent sol)
    {
        if (!TryComp<OrganFoodProcessorComponent>(stomachUid, out var foodProcessor))
            return false;

        if (!_solutionContainerSystem.ResolveSolution((stomachUid, sol), DefaultSolutionName, ref stomach.Solution, out var stomachSolution))
            return false;

        if (stomach.ReagentDeltas.Count == 0)
            return false;

        if (organ.Body is not { } body)
            return false;

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
            stomach.ReagentDeltas.Remove(item);

        _audio.PlayPvs(foodProcessor.ProcessingSound, body);
        _jittering.DoJitter(body, foodProcessor.JitterDuration, true, frequency: foodProcessor.JitterFrequency);

        return true;
    }
}
