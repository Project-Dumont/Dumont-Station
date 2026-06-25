using System.Linq;
using Content.Server.Body;
using Content.Server.Station.Systems;
using Content.Shared._FarHorizons.Factions;
using Content.Shared._FarHorizons.Salvage;
using Content.Shared._FarHorizons.Salvage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

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

        var factions = IoCManager.Resolve<ISharedFactionManager>();
        var visualBody = EntMan.System<VisualBodySystem>();
        var profile = EntMan.System<HumanoidProfileSystem>();
        var metadata = EntMan.System<MetaDataSystem>();
        var state = EntMan.System<MobStateSystem>();
        var damageable = EntMan.System<DamageableSystem>();
        var stationSpawning = EntMan.System<StationSpawningSystem>();
        var inventory = EntMan.System<InventorySystem>();
        var paper = EntMan.System<PaperSystem>();
        var disarmConsole = EntMan.System<SalvageMissionDisarmConsoleSystem>();

        var possibleFactions = factions.ListPlayableFactions().Where(p => p.Major).ToList();
        var selectedFaction = possibleFactions[Rand.Next(possibleFactions.Count)];

        List<EntityUid> bodies = [];

        for (var i = 0; i < NumBodies; i++)
        {
            if (GetRandomEmptyTileInDungeon() is not { } pos) return;

            var damage = SalvageMissionRescue.RandomDamage(ProtoMan, Rand, 100, 200, 4);
            var body = SalvageMissionRescue.SpawnRandomBody(ProtoMan, EntMan, Rand, pos, visualBody, profile, metadata, state, damageable, factions, stationSpawning, inventory, selectedFaction, true, damage, true);
            bodies.Add(body);
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
            var slot = _pocketSlots[Rand.Next(_pocketSlots.Count)];
            var body = bodies.Pop();
            var console = consoles.Pop();
            var spawnedPaper = EntMan.Spawn(_paper);

            if (!inventory.TryEquip(body, spawnedPaper, slot, force: true))
            {
                EntMan.DeleteEntity(spawnedPaper);
                i--;
                continue;
            }

            var code = GenerateCode();
            
            paper.SetContent(spawnedPaper, Loc.GetString("salvage-mission-objective-disarm-paper", ("code", code)));
            disarmConsole.SetupConsole(console.AsNullable(), code);
            MarkEntity(console);
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