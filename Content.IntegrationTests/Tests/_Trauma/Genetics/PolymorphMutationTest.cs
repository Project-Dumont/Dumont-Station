using System.Collections.Generic;
using System.Linq;
using Content.IntegrationTests.Pair;
using Content.Server.Polymorph.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Trauma.Shared.Genetics.Abilities;
using Content.Trauma.Shared.Genetics.Mutations;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Trauma.Genetics;

[TestFixture]
public sealed class PolymorphMutationTest
{
    private static readonly EntProtoId<MutationComponent> Monkificar = "MutationMonkified";
    private const string Compativel = "MobHarpy";
    private const string Incompativel = "MobDiona";
    private const string FormaMacaco = "MobMonkey";

    [Test]
    public async Task EspecieCompativelViraMacacoEVolta()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var mutation = entMan.System<MutationSystem>();

        EntityUid mob = default;
        await pair.Server.WaitPost(() => mob = entMan.SpawnEntity(Compativel, map.GridCoords));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(entMan.HasComponent<MutatableComponent>(mob), Is.True,
                $"{Compativel} não é mutável, então a genética nem chega nele");
            Assert.That(Polimorfos(entMan), Is.Empty, "já existia corpo polimorfado antes de começar");
        });

        await pair.Server.WaitPost(() => Assert.That(mutation.AddMutation(mob, Monkificar), "a mutação não entrou"));
        await pair.Server.WaitRunTicks(1);

        EntityUid macaco = default;
        await pair.Server.WaitAssertion(() =>
        {
            var polimorfos = Polimorfos(entMan);
            Assert.That(polimorfos, Has.Count.EqualTo(1), $"{Compativel} não virou nada");

            macaco = polimorfos[0];
            Assert.That(entMan.GetComponent<PolymorphedEntityComponent>(macaco).Parent, Is.EqualTo(mob),
                "o corpo que apareceu não veio do mob da montagem");
            Assert.That(entMan.GetComponent<MetaDataComponent>(macaco).EntityPrototype?.ID, Is.EqualTo(FormaMacaco),
                "a espécie compatível virou outra coisa que não macaco");
        });

        await pair.Server.WaitPost(() => mutation.RemoveMutation(macaco, Monkificar));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(entMan.EntityExists(macaco) && !entMan.IsPaused(macaco), Is.False,
                "tirar a mutação deixou o macaco de pé");
            Assert.That(Especie(entMan, mob), Is.EqualTo("Harpy"),
                "o corpo que voltou não é o da espécie original");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task EspecieIncompativelNaoTransformaENaoViraHumano()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var mutation = entMan.System<MutationSystem>();

        EntityUid mob = default;
        await pair.Server.WaitPost(() => mob = entMan.SpawnEntity(Incompativel, map.GridCoords));
        await pair.Server.WaitRunTicks(1);

        var especie = "";
        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(entMan.HasComponent<MutatableComponent>(mob), Is.True,
                $"{Incompativel} não é mutável, então este teste não mede recusa nenhuma");
            especie = Especie(entMan, mob);
        });

        await pair.Server.WaitPost(() => Assert.That(mutation.AddMutation(mob, Monkificar), "a mutação não entrou"));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(Polimorfos(entMan), Is.Empty, "a espécie incompatível foi transformada mesmo assim");
            Assert.That(Especie(entMan, mob), Is.EqualTo(especie), "a espécie mudou sem polimorfismo nenhum");
        });

        await pair.Server.WaitPost(() => mutation.RemoveMutation(mob, Monkificar));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(Polimorfos(entMan), Is.Empty,
                "tirar a mutação transformou quem ela nunca tinha tocado");
            Assert.That(Especie(entMan, mob), Is.EqualTo(especie),
                "tirar a mutação trocou a espécie de quem era incompatível");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TodaEspecieJogavelEstaDecidida()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var compFact = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var jogaveis = protoMan.EnumeratePrototypes<SpeciesPrototype>()
                .Where(s => s.RoundStart)
                .Select(s => s.ID)
                .ToList();
            Assert.That(jogaveis, Is.Not.Empty, "não achei espécie jogável nenhuma, então o teste não mede nada");

            var conferidas = 0;
            foreach (var proto in protoMan.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.Abstract || pair.IsTestPrototype(proto))
                    continue;

                if (!proto.TryGetComponent<PolymorphMutationComponent>(out var poly, compFact))
                    continue;

                if (poly.Incompatible.Count == 0)
                    continue;

                conferidas++;
                foreach (var especie in jogaveis)
                {
                    var decidida = poly.Prototypes.ContainsKey(especie) || poly.Incompatible.Contains(especie);
                    Assert.That(decidida, Is.True,
                        $"a mutação {proto.ID} não diz se {especie} é compatível: ponha em prototypes ou em incompatible");
                }
            }

            Assert.That(conferidas, Is.GreaterThan(0), "nenhuma mutação declarou incompatíveis, então nada foi conferido");
        });

        await pair.CleanReturnAsync();
    }


    [Test]
    public async Task ReptilianoViraKoboldEVolta()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var map = await pair.CreateTestMap();
        var entMan = pair.Server.ResolveDependency<IEntityManager>();
        var mutation = entMan.System<MutationSystem>();

        EntityUid mob = default;
        await pair.Server.WaitPost(() => mob = entMan.SpawnEntity("MobReptilian", map.GridCoords));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitPost(() => Assert.That(mutation.AddMutation(mob, Monkificar), "a mutação não entrou"));
        await pair.Server.WaitRunTicks(1);

        EntityUid kobold = default;
        await pair.Server.WaitAssertion(() =>
        {
            var polimorfos = Polimorfos(entMan);
            Assert.That(polimorfos, Has.Count.EqualTo(1), "o reptiliano não virou nada");

            kobold = polimorfos[0];
            Assert.That(entMan.GetComponent<MetaDataComponent>(kobold).EntityPrototype?.ID, Is.EqualTo("MobKobold"),
                "o reptiliano virou outra coisa que não kobold");
            Assert.That(entMan.HasComponent<MutatableComponent>(kobold), Is.True,
                "o kobold não é mutável, então não existe caminho de volta");
            Assert.That(mutation.HasMutation(kobold, Monkificar), Is.True,
                "a mutação não acompanhou o corpo novo, e sem ela o limpador não tem o que tirar");
        });

        await pair.Server.WaitPost(() => mutation.RemoveMutation(kobold, Monkificar));
        await pair.Server.WaitRunTicks(1);

        await pair.Server.WaitAssertion(() =>
            Assert.That(Especie(entMan, mob), Is.EqualTo("Reptilian"), "o corpo que voltou não é o reptiliano"));

        await pair.CleanReturnAsync();
    }

    private static List<EntityUid> Polimorfos(IEntityManager entMan)
    {
        var achados = new List<EntityUid>();
        var busca = entMan.EntityQueryEnumerator<PolymorphedEntityComponent>();
        while (busca.MoveNext(out var uid, out _))
        {
            achados.Add(uid);
        }
        return achados;
    }

    private static string Especie(IEntityManager entMan, EntityUid uid)
        => entMan.GetComponent<HumanoidAppearanceComponent>(uid).Species.Id;
}
