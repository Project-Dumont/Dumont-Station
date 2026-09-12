using System.Collections.Generic;
using System.Numerics;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Medical.Components;
using Content.Shared.CCVar;
using Content.Trauma.Shared.Genetics.Console;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Trauma.Genetics;

[TestFixture]
public sealed class GeneticsMapaTest
{
    private const string Mapa = "Meta";

    private static readonly Dictionary<Vector2, Vector2> ConsoleParaScanner = new()
    {
        [new Vector2(4.5f, -53.5f)] = new Vector2(4.5f, -52.5f),
        [new Vector2(4.5f, -50.5f)] = new Vector2(4.5f, -49.5f),
        [new Vector2(0.5f, -53.5f)] = new Vector2(0.5f, -52.5f),
    };

    [Test]
    public async Task ConsolesDoMapaSobemLigadosAoScannerCerto()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var ticker = entMan.System<GameTicker>();
        var cfg = server.ResolveDependency<IConfigurationManager>();
        Assert.That(cfg.GetCVar(CCVars.GridFill), Is.False);

        await server.WaitPost(() =>
        {
            var opts = DeserializationOptions.Default with { InitializeMaps = true };
            ticker.LoadGameMap(protoMan.Index<GameMapPrototype>(Mapa), out _, opts);
        });
        await server.WaitRunTicks(5);

        await server.WaitAssertion(() =>
        {
            var achados = new List<(EntityUid Uid, GeneticsScannerComponent Comp, Vector2 Pos)>();
            var busca = entMan.EntityQueryEnumerator<GeneticsScannerComponent, TransformComponent>();
            while (busca.MoveNext(out var uid, out var comp, out var xform))
            {
                achados.Add((uid, comp, xform.LocalPosition));
            }

            Assert.That(achados, Has.Count.EqualTo(ConsoleParaScanner.Count),
                $"{Mapa} devia subir com {ConsoleParaScanner.Count} consoles de genética");

            foreach (var (lugarConsole, lugarScanner) in ConsoleParaScanner)
            {
                var console = achados.Find(c => c.Pos.EqualsApprox(lugarConsole, 0.01));
                Assert.That(console.Uid, Is.Not.EqualTo(EntityUid.Invalid),
                    $"não subiu console de genética em {lugarConsole}, então o mapa mudou e este teste está velho");

                Assert.That(console.Comp.Scanner, Is.Not.Null,
                    $"o console de {lugarConsole} subiu sem scanner, então a ligação salva no mapa não foi lida");

                var scanner = console.Comp.Scanner!.Value;
                Assert.That(entMan.HasComponent<MedicalScannerComponent>(scanner), Is.True,
                    $"o console de {lugarConsole} ficou ligado em {scanner}, que não é scanner médico");

                var posScanner = entMan.GetComponent<TransformComponent>(scanner).LocalPosition;
                Assert.That(posScanner.EqualsApprox(lugarScanner, 0.01), Is.True,
                    $"o console de {lugarConsole} ligou no scanner de {posScanner}, e o do mapa fica em {lugarScanner}");

                Assert.That(entMan.GetComponent<MedicalScannerComponent>(scanner).ConnectedConsole, Is.EqualTo(console.Uid),
                    $"o scanner de {lugarScanner} não aponta de volta para o console de {lugarConsole}");
            }
        });

        await pair.CleanReturnAsync();
    }
}
