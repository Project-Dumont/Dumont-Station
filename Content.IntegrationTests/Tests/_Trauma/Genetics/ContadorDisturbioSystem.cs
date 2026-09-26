using System.Collections.Generic;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.EntityEffects;
using Content.Shared.GameTicking;
using Content.Trauma.Shared.EntityEffects;
using Robust.Shared.GameObjects;
using Robust.Shared.Reflection;

namespace Content.IntegrationTests.Tests._Trauma.Genetics;

[Reflect(false)]
public sealed class ContadorDisturbioSystem : EntitySystem
{
    private readonly List<(EntityUid Alvo, bool Remove, string? Reagente, FixedPoint2 Quantidade)> _vistos = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<GeneticDisorder>>(OnExecute);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _vistos.Clear());
    }

    private void OnExecute(ref ExecuteEntityEffectEvent<GeneticDisorder> ev)
    {
        var reagente = ev.Args is EntityEffectReagentArgs { Reagent: {} r } ? r.ID : null;
        var quantidade = ev.Args is EntityEffectReagentArgs { Source: {} fonte } && reagente != null
            ? fonte.GetTotalPrototypeQuantity(reagente)
            : FixedPoint2.Zero;
        _vistos.Add((ev.Args.TargetEntity, ev.Effect.Remove, reagente, quantidade));
    }

    public List<FixedPoint2> Quantidades(EntityUid alvo, bool remove, string reagente)
        => _vistos
            .Where(v => v.Alvo == alvo && v.Remove == remove && v.Reagente == reagente)
            .Select(v => v.Quantidade)
            .ToList();
}
