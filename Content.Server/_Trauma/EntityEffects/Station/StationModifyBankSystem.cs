// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Cargo.Systems;
using Content.Shared.Cargo.Components;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.EntityEffects.Station;
using Robust.Shared.Random;

namespace Content.Trauma.Server.EntityEffects.Station;

public sealed partial class StationModifyBankSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private CargoSystem _cargo = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<StationModifyBank>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<StationModifyBank> args)
    {
        var station = args.Args.TargetEntity;
        if (!TryComp<StationBankAccountComponent>(station, out var bank))
            return;

        var e = args.Effect;
        var money = _random.Next(e.Min, e.Max);
        _cargo.UpdateBankAccount((station, bank), money, e.Account);
    }
}
