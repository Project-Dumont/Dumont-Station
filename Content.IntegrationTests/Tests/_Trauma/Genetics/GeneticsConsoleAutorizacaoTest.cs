using System;
using System.Linq;
using System.Threading.Tasks;
using Content.Server.Power.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Materials;
using Content.Shared.Stunnable;
using Content.Trauma.Shared.Genetics.Console;
using Content.Trauma.Shared.Genetics.Tools;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.IntegrationTests.Tests._Trauma.Genetics;

[TestFixture]
public sealed class GeneticsConsoleAutorizacaoTest
{
    private const string Console = "ComputerGeneticsConsole";
    private const string Disco = "GeneticsDiskUnstableDna";
    private const int Semente = 20260912;

    private sealed record Bancada(IEntityManager EntMan, EntityUid Jogador, EntityUid ConsoleUid, NetEntity Net);

    [Test]
    public async Task ControlePositivoImprimeOInjetor()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true, DummyTicker = false, Dirty = true, ServerSeed = Semente });
        var f = await Montar(pair);
        var antes = await Biomassa(pair, f);

        await Imprimir(pair, f);

        var injetores = await ContarInjetores(pair, f.EntMan);
        var depois = await Biomassa(pair, f);
        Assert.Multiple(() =>
        {
            Assert.That(injetores, Is.EqualTo(1), "o caminho válido não imprimiu");
            Assert.That(depois, Is.LessThan(antes), "imprimiu sem cobrar biomassa");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task SemInterfaceNaoImprimeNemGasta()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true, DummyTicker = false, Dirty = true, ServerSeed = Semente });
        var f = await Montar(pair, abrirInterface: false);
        var antes = await Biomassa(pair, f);

        await Imprimir(pair, f, exigeInterface: false);

        var injetores = await ContarInjetores(pair, f.EntMan);
        var depois = await Biomassa(pair, f);
        Assert.Multiple(() =>
        {
            Assert.That(injetores, Is.Zero, "imprimiu com a interface fechada");
            Assert.That(depois, Is.EqualTo(antes), "gastou biomassa com a interface fechada");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task IncapacitadoNaoImprimeNemGasta()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true, DummyTicker = false, Dirty = true, ServerSeed = Semente });
        var f = await Montar(pair);
        var antes = await Biomassa(pair, f);

        await pair.Server.WaitPost(() =>
            f.EntMan.System<SharedStunSystem>().TryAddStunDuration(f.Jogador, TimeSpan.FromSeconds(30)));
        await pair.RunTicksSync(5);

        await Imprimir(pair, f, exigeInterface: false);

        var injetores = await ContarInjetores(pair, f.EntMan);
        var depois = await Biomassa(pair, f);
        Assert.Multiple(() =>
        {
            Assert.That(injetores, Is.Zero, "imprimiu com o jogador incapacitado");
            Assert.That(depois, Is.EqualTo(antes), "gastou biomassa com o jogador incapacitado");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task SemEnergiaNaoImprimeNemGasta()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true, DummyTicker = false, Dirty = true, ServerSeed = Semente });
        var f = await Montar(pair);
        var antes = await Biomassa(pair, f);

        await pair.Server.WaitPost(() =>
            f.EntMan.System<PowerReceiverSystem>().SetNeedsPower(f.ConsoleUid, true));
        await pair.RunTicksSync(15);

        await pair.Server.WaitAssertion(() =>
            Assert.That(f.EntMan.System<PowerReceiverSystem>().IsPowered(f.ConsoleUid), Is.False,
                "o console continuou energizado, então o teste não mede falta de energia"));

        await Imprimir(pair, f, exigeInterface: false);

        var injetores = await ContarInjetores(pair, f.EntMan);
        var depois = await Biomassa(pair, f);
        Assert.Multiple(() =>
        {
            Assert.That(injetores, Is.Zero, "imprimiu com o console sem energia");
            Assert.That(depois, Is.EqualTo(antes), "gastou biomassa com o console sem energia");
        });

        await pair.CleanReturnAsync();
    }

    private static async Task<Bancada> Montar(Pair.TestPair pair, bool abrirInterface = true, bool comEnergia = true)
    {
        var server = pair.Server;
        var sEntMan = server.ResolveDependency<IEntityManager>();
        var cEntMan = pair.Client.ResolveDependency<IEntityManager>();
        var session = server.ResolveDependency<IPlayerManager>().Sessions.Single();
        var power = server.System<PowerReceiverSystem>();
        var ui = server.System<UserInterfaceSystem>();
        var tempo = server.ResolveDependency<IGameTiming>();

        var jogador = session.AttachedEntity!.Value;
        EntityUid console = default;
        NetEntity net = default;

        await server.WaitPost(() =>
        {
            sEntMan.RemoveComponent<StunnedComponent>(jogador);

            var coords = sEntMan.GetComponent<TransformComponent>(jogador).Coordinates;
            console = sEntMan.SpawnEntity(Console, coords);
            net = sEntMan.GetNetEntity(console);

            if (comEnergia)
                power.SetNeedsPower(console, false);

            var disco = sEntMan.SpawnEntity(Disco, coords);
            sEntMan.System<ItemSlotsSystem>().TryInsert(console, "genetics_disk", disco, null);
        });

        await pair.RunTicksSync(15);

        await pair.Client.WaitAssertion(() =>
        {
            var cConsole = cEntMan.GetEntity(net);
            Assert.That(cEntMan.EntityExists(cConsole), Is.True, "o console não replicou para o cliente");
            Assert.That(cEntMan.HasComponent<UserInterfaceComponent>(cConsole), Is.True,
                "o UserInterfaceComponent do console ainda não existe no cliente");
        });

        if (abrirInterface)
            await server.WaitPost(() => ui.OpenUi(console, GeneticsConsoleUiKey.Key, jogador));

        await pair.RunTicksSync(15);

        while (tempo.CurTime < sEntMan.GetComponent<GeneticsConsoleComponent>(console).NextPrint)
        {
            await pair.RunTicksSync(30);
        }

        return new Bancada(sEntMan, jogador, console, net);
    }

    private static async Task Imprimir(Pair.TestPair pair, Bancada f, bool exigeInterface = true)
    {
        var cEntMan = pair.Client.ResolveDependency<IEntityManager>();
        await pair.Client.WaitPost(() =>
        {
            var cConsole = cEntMan.GetEntity(f.Net);
            if (!cEntMan.TryGetComponent<UserInterfaceComponent>(cConsole, out var ui)
                || !ui.ClientOpenInterfaces.TryGetValue(GeneticsConsoleUiKey.Key, out var bui))
            {
                if (exigeInterface)
                    Assert.Fail("a interface do console não está aberta no cliente");
                return;
            }

            bui.SendMessage(new GeneticsConsolePrintMessage(0));
        });
        await pair.RunTicksSync(15);
    }

    private static async Task<int> ContarInjetores(Pair.TestPair pair, IEntityManager entMan)
    {
        var total = 0;
        await pair.Server.WaitPost(() =>
        {
            var query = entMan.EntityQueryEnumerator<MutatorComponent>();
            while (query.MoveNext(out _, out _))
            {
                total++;
            }
        });
        return total;
    }

    private static async Task<int> Biomassa(Pair.TestPair pair, Bancada f)
    {
        var total = 0;
        await pair.Server.WaitPost(() =>
        {
            var comp = f.EntMan.GetComponent<GeneticsConsoleComponent>(f.ConsoleUid);
            total = f.EntMan.System<SharedMaterialStorageSystem>().GetMaterialAmount(f.ConsoleUid, comp.Biomass);
        });
        return total;
    }
}
