// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Silicons.Laws;
using Content.Shared.EntityEffects;
using Content.Shared.Silicons.Laws.Components;
using Content.Trauma.Shared.EntityEffects;
using Robust.Shared.Random;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class RandomizeLawsetSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SiliconLawSystem _law = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<RandomizeLawset>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<RandomizeLawset> args)
    {
        var uid = args.Args.TargetEntity;
        if (!HasComp<SiliconLawProviderComponent>(uid))
            return;

        var lawset = _random.Pick(args.Effect.Lawsets);
        var laws = _law.GetLawset(lawset).Laws;
        _law.SetLaws(laws, uid);
    }
}
