// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Spawners.Components;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.EntityEffects.Conditions;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class SpawnPointHasJobSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CheckEntityEffectConditionEvent<SpawnPointHasJob>>(OnCondition);
    }

    private void OnCondition(ref CheckEntityEffectConditionEvent<SpawnPointHasJob> args)
    {
        args.Result = TryComp<SpawnPointComponent>(args.Args.TargetEntity, out var point) && point.Job != null;
    }
}
