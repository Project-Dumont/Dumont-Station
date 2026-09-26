// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs.Components;

namespace Content.Shared.Mobs.Systems;

public sealed partial class MobThresholdSystem
{
    /// <summary>
    /// Force mob into a state and keep his thresholds there, so bandage cant make him normal again (For some reason) Good when you need
    /// some mob dead but his damage dont reach the death threshold by his own, Defib still can bring him back normal like before.
    /// ok
    /// </summary>
    public void ForceThresholdState(EntityUid target, MobState state, MobThresholdsComponent? thresholds = null, MobStateComponent? mobState = null)
    {
        if (!Resolve(target, ref thresholds, ref mobState, false))
            return;

        thresholds.CurrentThresholdState = state;
        Dirty(target, thresholds);

        _mobStateSystem.ChangeMobState(target, state, mobState);
    }
}
