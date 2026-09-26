// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Trauma.Shared.Genetics.Mutations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Trauma.Server.Genetics;

public sealed partial class MutagenicRadiationSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MutationSystem _mutation = default!;

    private const float Chance = 0.05f;

    private static readonly FixedPoint2 DoseMinima = FixedPoint2.New(1);

    private static readonly ProtoId<DamageTypePrototype> Radiacao = "Radiation";

    private readonly Dictionary<EntityUid, FixedPoint2> _dose = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MutatableComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<MutatableComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta is not {} delta)
            return;

        if (!delta.DamageDict.TryGetValue(Radiacao, out var dose) || dose <= FixedPoint2.Zero)
            return;

        _dose[ent.Owner] = _dose.GetValueOrDefault(ent.Owner) + dose;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_dose.Count == 0)
            return;

        foreach (var (uid, dose) in _dose)
        {
            if (dose < DoseMinima || !_random.Prob(Chance))
                continue;

            if (!TryComp<MutatableComponent>(uid, out var comp))
                continue;

            _mutation.AddRandomDisorder((uid, comp));
        }

        _dose.Clear();
    }
}
