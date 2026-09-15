using System.Collections.Generic;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.IntegrationTests.Pair;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.EffectConditions;
using Content.Shared.Traits;
using Content.Shared.Traits.Assorted;
using Content.Trauma.Server.Genetics;
using Content.Trauma.Shared.EntityEffects;
using Content.Trauma.Shared.Genetics.Mutations;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Trauma.Genetics;

[TestFixture]
public sealed class MigracaoGeneticaAntigaTest
{
    private const string Mob = "TestMigracaoMob";
    private const string Cego = "TestMigracaoCegoMob";
    private const string Traco = "DnaModifierDeviation";
    private const string Antigo = "DnaModifier";
    private const string Mutagenico = "UnstableMutagen";
    private const string Mutadon = "Mutadon";
    private static readonly EntProtoId<MutationComponent> Cegueira = "MutationBlindness";
    private static readonly EntProtoId<MutationComponent> Forca = "MutationStrength";
    private static readonly EntProtoId<MutationComponent> SuorDeFogo = "MutationFierySweat";
    private static readonly ProtoId<DamageTypePrototype> Radiacao = "Radiation";
    private static readonly ProtoId<DamageTypePrototype> Pancada = "Blunt";

    private static readonly string[] ForaDaLista =
    {
        "MutationUnintelligible",
        "MutationStoner",
        "MutationThermalWeakness",
        "MutationSpastic",            // ataca outras pessoas
        "MutationRadioactivity",      // irradia outras pessoas
        "MutationInternalMartyrdom",
        "MutationFierySweat",         // reduz dano recebido
        "MutationSpatialInstability",
        "MutationHeadless",
        "MutationAcromegaly",
        "MutationFelinized",
    };

    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  parent: MobHuman
  id: TestMigracaoMob
  components:
  - type: Mutatable
    maxDormant: 0

- type: entity
  parent: TestMigracaoMob
  id: TestMigracaoCegoMob
  components:
  - type: GeneticDeviation
    mutations:
    - MutationBlindness
    min: 1
    max: 1
";

    [Test]
    public async Task AListaDeDisturbiosEAQueFoiMarcada()
    {
        await using var pair = await PoolManager.GetServerClient();
        var proto = pair.Server.ResolveDependency<IPrototypeManager>();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var mutation = entMan.System<MutationSystem>();
        var factory = pair.Server.ResolveDependency<IComponentFactory>();

        await pair.Server.WaitAssertion(() =>
        {
            var nome = factory.GetComponentName<GeneticDisorderComponent>();
            var marcadas = proto.EnumeratePrototypes<EntityPrototype>()
                .Where(p => !p.Abstract && p.Components.ContainsKey(nome))
                .Select(p => (EntProtoId<MutationComponent>) p.ID)
                .ToHashSet();

            Assert.Multiple(() =>
            {
                Assert.That(marcadas, Is.Not.Empty, "nenhuma mutação está marcada, o teste não mede nada");
                Assert.That(mutation.Disorders.ToHashSet(), Is.EquivalentTo(marcadas),
                    "a lista de distúrbios não é a das mutações marcadas, alguma caiu por bloqueio ou receita");

                foreach (var id in ForaDaLista)
                    Assert.That(mutation.IsDisorder(id), Is.False, $"{id} entrou na lista e fere um critério dela");
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OsDoisReagentesFalamComAGeneticaNova()
    {
        await using var pair = await PoolManager.GetServerClient();
        var proto = pair.Server.ResolveDependency<IPrototypeManager>();

        await pair.Server.WaitAssertion(() =>
        {
            var mutagenico = Entrada(proto, Mutagenico).Effects;
            var mutadon = Entrada(proto, Mutadon).Effects;

            Assert.Multiple(() =>
            {
                Assert.That(mutagenico.OfType<GeneticDisorder>().Any(e => !e.Remove), Is.True,
                    "o mutagênico não causa distúrbio pela genética nova");
                Assert.That(mutadon.OfType<GeneticDisorder>().Any(e => e.Remove), Is.True,
                    "o Mutadon não cura distúrbio pela genética nova");

                foreach (var (nome, efeitos) in new[] { (Mutagenico, mutagenico), (Mutadon, mutadon) })
                {
                    var antigos = efeitos
                        .Select(e => e.GetType().Name)
                        .Where(n => n is "ChemMutateDna" or "ChemCureDnaDisease")
                        .ToList();
                    Assert.That(antigos, Is.Empty, $"{nome} ainda chama a genética antiga: {string.Join(", ", antigos)}");
                }
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OEfeitoCausaECuraDisturbio()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var mutation = entMan.System<MutationSystem>();

        EntityUid mob = default;
        await pair.Server.WaitPost(() => mob = entMan.SpawnEntity(Mob, map.GridCoords));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(Disturbios(mutation, mob), Is.Empty, "o mob já nasceu com distúrbio");
            var antes = mutation.GetInstability(mob);

            Aplicar(entMan, mob, remove: false);
            Assert.That(Disturbios(mutation, mob), Has.Count.EqualTo(1), "o efeito não causou distúrbio nenhum");

            for (var i = 0; i < 40; i++)
                Aplicar(entMan, mob, remove: true);

            Assert.Multiple(() =>
            {
                Assert.That(Disturbios(mutation, mob), Is.Empty, "o efeito não curou o distúrbio");
                Assert.That(mutation.GetInstability(mob), Is.EqualTo(antes),
                    "curar o distúrbio não devolveu a instabilidade ao valor de antes");
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OMutadonNaoTiraMutacaoForaDaLista()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var mutation = entMan.System<MutationSystem>();

        EntityUid mob = default;
        await pair.Server.WaitPost(() => mob = entMan.SpawnEntity(Mob, map.GridCoords));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
        {
            foreach (var id in new[] { Forca, SuorDeFogo })
            {
                Assert.That(mutation.IsDisorder(id), Is.False, $"{id} está na lista, o controle não mede nada");
                Assert.That(mutation.AddMutation(mob, id), Is.True, $"{id} não entrou");
            }

            var antes = mutation.GetInstability(mob);
            for (var i = 0; i < 40; i++)
                Aplicar(entMan, mob, remove: true);

            Assert.Multiple(() =>
            {
                Assert.That(mutation.HasMutation(mob, Forca), Is.True, "o Mutadon tirou a Força");
                Assert.That(mutation.HasMutation(mob, SuorDeFogo), Is.True, "o Mutadon tirou o Suor de Fogo");
                Assert.That(mutation.GetInstability(mob), Is.EqualTo(antes), "o Mutadon mexeu na instabilidade");
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OMutagenicoPeloMetabolismoRespeitaOLimiarEOConsumo()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var proto = pair.Server.ResolveDependency<IPrototypeManager>();
        var mutation = entMan.System<MutationSystem>();
        var bloodstream = entMan.System<BloodstreamSystem>();

        var (taxa, limiar) = TaxaELimiar(proto);
        var acima = limiar + FixedPoint2.New(10);
        var abaixo = limiar - taxa;
        var esperadoAcima = Esperado(acima, taxa, limiar);
        var esperadoAbaixo = Esperado(abaixo, taxa, limiar);

        EntityUid alto = default;
        EntityUid baixo = default;
        var contador = entMan.System<ContadorDisturbioSystem>();
        await pair.Server.WaitPost(() =>
        {
            alto = entMan.SpawnEntity(Mob, map.GridCoords);
            baixo = entMan.SpawnEntity(Mob, map.GridCoords);
        });
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitPost(() =>
        {
            Assert.That(bloodstream.TryAddToChemicals(alto, new Solution(Mutagenico, acima)), Is.True);
            Assert.That(bloodstream.TryAddToChemicals(baixo, new Solution(Mutagenico, abaixo)), Is.True);
        });

        await pair.RunSeconds(esperadoAcima.Count + 4);

        await pair.Server.WaitAssertion(() =>
        {
            var vistoAcima = contador.Quantidades(alto, remove: false, Mutagenico);
            var vistoAbaixo = contador.Quantidades(baixo, remove: false, Mutagenico);
            var entraram = Disturbios(mutation, alto).Count;
            TestContext.Out.WriteLine($"dose {acima}: {vistoAcima.Count} execuções, {entraram} distúrbios presentes no fim");

            Assert.Multiple(() =>
            {
                Assert.That(esperadoAcima, Has.Count.EqualTo(21), "a conta das regras mudou, conferir limiar e taxa");
                Assert.That(vistoAcima, Is.EqualTo(esperadoAcima),
                    "as execuções não seguiram o limiar inclusivo e o consumo por ciclo");
                Assert.That(vistoAbaixo, Is.EqualTo(esperadoAbaixo),
                    "abaixo do limiar o mutagênico executou");
                Assert.That(entraram, Is.GreaterThan(0), "as execuções não puseram distúrbio nenhum");
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task ExecutarNaoEOMesmoQueEntrar()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var proto = pair.Server.ResolveDependency<IPrototypeManager>();
        var mutation = entMan.System<MutationSystem>();
        var bloodstream = entMan.System<BloodstreamSystem>();

        var (taxa, limiar) = TaxaELimiar(proto);
        var dose = limiar + taxa;
        var esperado = Esperado(dose, taxa, limiar);

        EntityUid mob = default;
        var contador = entMan.System<ContadorDisturbioSystem>();
        await pair.Server.WaitPost(() =>
        {
            mob = entMan.SpawnEntity(Mob, map.GridCoords);
        });
        await pair.Server.WaitRunTicks(1);

        HashSet<EntProtoId<MutationComponent>> antes = new();
        await pair.Server.WaitPost(() =>
        {
            Saturar(entMan, mutation, mob);
            antes = Disturbios(mutation, mob).ToHashSet();
            Assert.That(bloodstream.TryAddToChemicals(mob, new Solution(Mutagenico, dose)), Is.True);
        });

        await pair.RunSeconds(esperado.Count + 4);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(contador.Quantidades(mob, remove: false, Mutagenico), Is.EqualTo(esperado),
                    "o mutagênico não executou o que as regras mandam");
                Assert.That(Disturbios(mutation, mob).ToHashSet(), Is.EquivalentTo(antes),
                    "entrou distúrbio num genoma que já não tinha espaço");
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OMutadonPeloMetabolismoCuraUmPorCiclo()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var proto = pair.Server.ResolveDependency<IPrototypeManager>();
        var mutation = entMan.System<MutationSystem>();
        var bloodstream = entMan.System<BloodstreamSystem>();

        var taxa = Entrada(proto, Mutadon).MetabolismRate;
        var dose = taxa * 2;
        var esperado = Esperado(dose, taxa, FixedPoint2.Epsilon);

        EntityUid mob = default;
        var contador = entMan.System<ContadorDisturbioSystem>();
        await pair.Server.WaitPost(() =>
        {
            mob = entMan.SpawnEntity(Mob, map.GridCoords);
        });
        await pair.Server.WaitRunTicks(1);

        var antes = 0;
        await pair.Server.WaitPost(() =>
        {
            Saturar(entMan, mutation, mob);
            antes = Disturbios(mutation, mob).Count;
            Assert.That(bloodstream.TryAddToChemicals(mob, new Solution(Mutadon, dose)), Is.True);
        });

        await pair.RunSeconds(esperado.Count + 4);

        await pair.Server.WaitAssertion(() =>
        {
            var execucoes = contador.Quantidades(mob, remove: true, Mutadon);

            Assert.Multiple(() =>
            {
                Assert.That(antes, Is.GreaterThan(esperado.Count), "o corpo não tinha distúrbio bastante para medir");
                Assert.That(execucoes, Is.EqualTo(esperado), "o Mutadon não executou o que as regras mandam");
                Assert.That(antes - Disturbios(mutation, mob).Count, Is.EqualTo(execucoes.Count),
                    "cada execução do Mutadon não tirou exatamente um distúrbio");
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OTracoCausaADeficienciaDesdeOComeco()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var mutation = entMan.System<MutationSystem>();

        EntityUid mob = default;
        await pair.Server.WaitPost(() => mob = entMan.SpawnEntity(Cego, map.GridCoords));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
            Assert.Multiple(() =>
            {
                Assert.That(mutation.HasMutation(mob, Cegueira), Is.True,
                    "o traço não ativou a mutação, só a deixou dormente");
                Assert.That(entMan.HasComponent<PermanentBlindnessComponent>(mob), Is.True,
                    "a mutação entrou mas o mob não ficou cego, então o traço não causa a deficiência");
            }));

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OTracoDaDisturbioQueAGeneticaNovaEnxerga()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var proto = pair.Server.ResolveDependency<IPrototypeManager>();
        var mutation = entMan.System<MutationSystem>();

        EntityUid mob = default;
        await pair.Server.WaitPost(() => mob = entMan.SpawnEntity(Mob, map.GridCoords));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitPost(() =>
        {
            var traco = proto.Index<TraitPrototype>(Traco);
            entMan.AddComponents(mob, traco.Components, false);
        });
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
        {
            var comp = entMan.GetComponent<GeneticDeviationComponent>(mob);
            var pegou = comp.Mutations.Where(id => mutation.HasMutation(mob, id)).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(pegou, Has.Count.InRange(comp.Min, comp.Max),
                    "o traço não deu a quantidade de mutações que promete");
                Assert.That(mutation.GetInstability(mob), Is.Zero,
                    "o desvio de nascença cobrou instabilidade, e ele é parte do genoma");

                var fora = comp.Mutations.Where(id => !mutation.IsDisorder(id)).ToList();
                Assert.That(fora, Is.Empty, $"o traço sorteia mutação fora da lista de distúrbios: {string.Join(", ", fora)}");

                var mutatable = entMan.GetComponent<MutatableComponent>(mob);
                foreach (var id in pegou)
                {
                    Assert.That(mutation.IsForeign(mutatable, id), Is.False,
                        $"{id} ficou como mutação estrangeira, então o sequenciador não vai achá-la");
                }
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task RadiacaoMutaEPancadaNao()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var mutation = entMan.System<MutationSystem>();
        var damage = entMan.System<DamageableSystem>();

        var irradiados = new List<EntityUid>();
        var apanhados = new List<EntityUid>();
        await pair.Server.WaitPost(() =>
        {
            for (var i = 0; i < 20; i++)
            {
                irradiados.Add(entMan.SpawnEntity(Mob, map.GridCoords));
                apanhados.Add(entMan.SpawnEntity(Mob, map.GridCoords));
            }
        });
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
            Assert.That(irradiados.Count(m => Disturbios(mutation, m).Count > 0), Is.Zero, "algum mob já nasceu mutado"));

        for (var volta = 0; volta < 30; volta++)
        {
            await pair.Server.WaitPost(() =>
            {
                foreach (var mob in irradiados)
                    Bater(damage, mob, Radiacao);
                foreach (var mob in apanhados)
                    Bater(damage, mob, Pancada);
            });
            await pair.Server.WaitRunTicks(1);
        }

        await pair.Server.WaitAssertion(() =>
            Assert.Multiple(() =>
            {
                Assert.That(irradiados.Count(m => Disturbios(mutation, m).Count > 0), Is.GreaterThan(0),
                    "600 tiques de radiação não renderam distúrbio nenhum");
                Assert.That(apanhados.Count(m => Disturbios(mutation, m).Count > 0), Is.Zero,
                    "pancada mutou, e só radiação deveria mutar");
            }));

        await pair.CleanReturnAsync();
    }

    private static List<FixedPoint2> Esperado(FixedPoint2 dose, FixedPoint2 taxa, FixedPoint2 limiar)
    {
        var lista = new List<FixedPoint2>();
        for (var q = dose; q >= limiar && q > FixedPoint2.Zero; q -= taxa)
            lista.Add(q);
        return lista;
    }

    private static (FixedPoint2 Taxa, FixedPoint2 Limiar) TaxaELimiar(IPrototypeManager proto)
    {
        var metabolismos = proto.Index<ReagentPrototype>(Mutagenico).Metabolisms!;
        var remedio = metabolismos["Medicine"];

        foreach (var grupo in new[] { "Poison", "Narcotic" })
        {
            if (metabolismos.TryGetValue(grupo, out var outro))
                Assert.That(outro.MetabolismRate, Is.EqualTo(remedio.MetabolismRate), $"a taxa de {grupo} difere, refazer a conta");
        }

        var limiar = remedio.Effects.OfType<GeneticDisorder>().Single().Conditions!.OfType<ReagentThreshold>().Single().Min;
        return (remedio.MetabolismRate, limiar);
    }

    private static ReagentEffectsEntry Entrada(IPrototypeManager proto, string reagente)
    {
        var metabolismos = proto.Index<ReagentPrototype>(reagente).Metabolisms;
        Assert.That(metabolismos, Is.Not.Null, $"{reagente} não metaboliza nada");
        return metabolismos!["Medicine"];
    }

    private static void Saturar(IEntityManager entMan, MutationSystem mutation, EntityUid mob)
    {
        var comp = entMan.GetComponent<MutatableComponent>(mob);
        var voltas = 0;
        while (mutation.AddRandomDisorder((mob, comp)))
        {
            voltas++;
            Assert.That(voltas, Is.LessThanOrEqualTo(mutation.Disorders.Count), "saturar não terminou");
        }
    }

    private static void Aplicar(IEntityManager entMan, EntityUid mob, bool remove)
    {
        new GeneticDisorder { Remove = remove }.Effect(new EntityEffectBaseArgs(mob, entMan));
    }

    private static void Bater(DamageableSystem damage, EntityUid mob, ProtoId<DamageTypePrototype> tipo)
    {
        damage.TryChangeDamage(mob, new DamageSpecifier { DamageDict = { [tipo] = 2 } }, ignoreResistances: true);
    }

    private static List<EntProtoId<MutationComponent>> Disturbios(MutationSystem mutation, EntityUid mob)
        => mutation.Disorders.Where(id => mutation.HasMutation(mob, id)).ToList();
}
