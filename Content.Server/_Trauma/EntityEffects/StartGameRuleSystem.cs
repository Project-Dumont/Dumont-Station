// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.EntityEffects;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class StartGameRuleSystem : EntitySystem
{
    [Dependency] private GameTicker _ticker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<StartGameRule>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<StartGameRule> args)
    {
        _ticker.StartGameRule(args.Effect.Rule);
    }
}
