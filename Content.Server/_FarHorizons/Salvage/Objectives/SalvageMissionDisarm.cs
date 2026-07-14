using System.Linq;
using Content.Server.Body;
using Content.Server.Station.Systems;
using Content.Shared._FarHorizons.Salvage;
using Content.Shared._FarHorizons.Salvage.Components;
using Content.Shared.Damage.Systems;
using Robust.Shared.Map;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Shared.Damage;
using Content.Server.Humanoid;

namespace Content.Server._FarHorizons.Salvage.Objectives;

public sealed partial class SalvageMissionDisarm : BaseSalvageMissionObjectiveHandler
{
    [DataField] public int NumBodies = 20;
    [DataField] public int CodeLength = 3;
    private static readonly EntProtoId _paper = "Paper";
    static readonly List<string> _pocketSlots = ["pocket1", "pocket2"];

    public override void AFterFTLToMap(EntityUid shuttle) => 
        Announce(GetAnnouncement());
        
    public override void BeforeFTLFromMap(EntityUid shuttle)
    {
        if (GetExpeditionConsole(shuttle) is not EntityUid expedConsole)
            return;
        
        var allTargets = GetAllMarkedEntities();
        var targetsDisarmed = allTargets.Count(p => EntMan.TryGetComponent<SalvageMissionDisarmConsoleComponent>(p, out var console) && !console.Armed);
        SetRewardComponent(expedConsole, ResolveCompletion(targetsDisarmed));
    }
    
    public override void BeforeFTLToMap(EntityUid shuttle){} // Override intentionally left empty

    public override void OnMapCreated()
    {
        if (!EntMan.TryGetComponent<TransformComponent>(Map, out var mapTransform))
            return;

        var humanoid = EntMan.System<HumanoidAppearanceSystem>();
        var metadata = EntMan.System<MetaDataSystem>();
        var state = EntMan.System<MobStateSystem>();
        var damageable = EntMan.System<DamageableSystem>();
        var inventory = EntMan.System<InventorySystem>();
        var paper = EntMan.System<PaperSystem>();
        var disarmConsole = EntMan.System<SalvageMissionDisarmConsoleSystem>();

        List<EntityUid> bodies = [];
        List<EntityCoordinates> bodyPositions = [];

        for (var i = 0; i < NumBodies; i++)
        {
            if (GetRandomEmptyTileInDungeon() is not { } pos) 
                continue; // Dumont

            // Dumont
            var damage = SalvageMissionRescue.RandomDamage(ProtoMan, Rand, 100, 200, 4);
            
            // Dumont
            var body = SalvageMissionRescue.SpawnRandomBody(ProtoMan, EntMan, Rand, pos, humanoid, metadata, inventory, state, damageable, true, damage, true);
            bodies.Add(body);
            bodyPositions.Add(pos); // Dumont
        }
        
        Rand.Shuffle(bodies);
        var numCodes = Objective.NumTargets.GetValueOrDefault(Difficulty, 0);

        var consoles = new List<Entity<SalvageMissionDisarmConsoleComponent>>();
        var enumerator = mapTransform.ChildEnumerator;

        while (enumerator.MoveNext(out var uid))
        {
            if (!EntMan.TryGetComponent<SalvageMissionDisarmConsoleComponent>(uid, out var console))
                continue;

            consoles.Add((uid, console));
        }
        Rand.Shuffle(consoles);

        for (var i = 0; i < numCodes; i++)
        {
            if (bodies.Count == 0 || consoles.Count == 0)
                break; // Dumont

            var slot = _pocketSlots[Rand.Next(_pocketSlots.Count)];
            
            // Dumont
            var body = bodies.Pop();
            var console = consoles.Pop();
            
            // Dumont
            var bodyXform = EntMan.GetComponent<TransformComponent>(body);
            var pos = bodyXform.Coordinates;

            // Dumont
            var spawnedPaper = EntMan.SpawnAtPosition(_paper, pos);

            var code = GenerateCode();
            
            paper.SetContent(spawnedPaper, Loc.GetString("salvage-mission-objective-disarm-paper", ("code", code)));
            disarmConsole.SetupConsole(console.AsNullable(), code);
            MarkEntity(console);

            // Dumont
            inventory.TryEquip(body, spawnedPaper, slot, force: true);
        }
    }

    public int GenerateCode()
    {
        var result = 0;

        for (var i = 0; i < CodeLength; i++)
        {
            var digit = Rand.Next(0, 10);

            result *= 10;
            result += digit;
        }

        return result;
    }
}