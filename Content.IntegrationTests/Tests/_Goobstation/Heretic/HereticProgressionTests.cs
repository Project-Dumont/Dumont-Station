using System.Linq;
using Content.Shared.Mind;
using Content.Trauma.Server.Heretic.Systems;
using Content.Trauma.Shared.Heretic.Components;
using Content.Trauma.Shared.Heretic.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture]
public sealed class HereticProgressionTests
{
    [Test]
    public async Task PathsUnlockKnowledgeBeforeAscension()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        await server.WaitAssertion(() =>
        {
            var entMan = server.EntMan;
            var session = server.ResolveDependency<ISharedPlayerManager>().Sessions.Single();
            var minds = entMan.System<SharedMindSystem>();
            var rules = entMan.System<HereticRuleSystem>();
            var heretics = entMan.System<HereticSystem>();
            foreach (var path in Enum.GetValues<HereticPath>())
            {
                var body = entMan.SpawnEntity("MobHuman", map.GridCoords);
                var mind = minds.CreateMind(session.UserId);
                minds.TransferTo(mind, body, mind: mind.Comp);
                rules.InitializeStore(mind);
                var heretic = entMan.AddComponent<HereticComponent>(mind);
                var knowledge = server.ProtoMan.EnumeratePrototypes<HereticKnowledgePrototype>()
                    .Where(value => value.Path == path && !value.SideKnowledge && value.Stage < 10)
                    .OrderBy(value => value.Stage).ToArray();
                Assert.That(knowledge, Is.Not.Empty, path.ToString());
                foreach (var entry in knowledge)
                {
                    Assert.That(heretics.TryAddKnowledge((mind, mind.Comp, heretic), entry.ID), Is.True, entry.ID);
                    foreach (var ritual in entry.RitualPrototypes ?? [])
                        Assert.That(heretic.RitualContainer.ContainedEntities.Any(entity =>
                            entMan.GetComponent<MetaDataComponent>(entity).EntityPrototype?.ID == ritual.Id), Is.True, ritual.Id);
                }
                Assert.That(heretic.CurrentPath, Is.EqualTo(path));
                Assert.That(heretic.PathStage, Is.EqualTo(knowledge.Max(value => value.Stage)));
                minds.WipeMind(mind, mind.Comp);
                entMan.DeleteEntity(body);
                entMan.DeleteEntity(mind);
            }
        });
        await pair.CleanReturnAsync();
    }
}
