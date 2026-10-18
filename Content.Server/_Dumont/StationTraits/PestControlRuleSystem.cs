using Content.Server.GameTicking.Rules;
using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Shared._Dumont.StationTraits;

namespace Content.Server._Dumont.StationTraits;

public sealed partial class PestControlRuleSystem : GameRuleSystem<PestControlRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MouseSpawnerComponent, MapInitEvent>(OnMapInit, before: [ typeof(ConditionalSpawnerSystem) ]);
    }

    private void OnMapInit(Entity<MouseSpawnerComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<RandomSpawnerComponent>(ent, out var spawner))
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var comp, out _))
        {
            spawner.Chance *= comp.SpawnChance;
            spawner.RareChance = 0f;
        }
    }
}
