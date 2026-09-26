// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Shared.Genetics.Mutations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Trauma.Server.Genetics;

/// <inheritdoc cref="GeneticDeviationComponent"/>
public sealed partial class GeneticDeviationSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MutationSystem _mutation = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneticDeviationComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GeneticDeviationComponent, MapInitEvent>(OnMapInit, after: new[] { typeof(MutationSystem) });
    }

    private void OnStartup(Entity<GeneticDeviationComponent> ent, ref ComponentStartup args)
    {
        if (MetaData(ent).EntityLifeStage >= EntityLifeStage.MapInitialized)
            Sortear(ent);
    }

    private void OnMapInit(Entity<GeneticDeviationComponent> ent, ref MapInitEvent args)
    {
        Sortear(ent);
    }

    private void Sortear(Entity<GeneticDeviationComponent> ent)
    {
        if (ent.Comp.Sorteado || _mutation.GetMutatable(ent.Owner, force: false) is not {} mutatable)
            return;

        ent.Comp.Sorteado = true;

        var sorteio = new List<EntProtoId<MutationComponent>>(ent.Comp.Mutations);
        _random.Shuffle(sorteio);

        var quantas = _random.Next(ent.Comp.Min, ent.Comp.Max + 1);
        foreach (var id in sorteio)
        {
            if (quantas <= 0)
                break;

            _mutation.AddDormant(mutatable, id);
            if (_mutation.ActivateMutation(mutatable.AsNullable(), id, automatic: true))
                quantas--;
        }
    }
}
