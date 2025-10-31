
using System.Linq;
using Content.Server.Humanoid;
using Content.Server.Station.Systems;
using Content.Shared._FarHorizons.Factions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Microsoft.CodeAnalysis;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._FarHorizons.Salvage.Objectives;

public sealed partial class SalvageMissionRescue : BaseSalvageMissionObjectiveHandler
{
    const int DecoyBodies = 20;
    const int TotalDamageForBonus = 100;
    static readonly EntProtoId GasMask = "ClothingMaskGas";
    const string MaskSlot = "mask";

    public override void AFterFTLToMap(EntityUid shuttle)
    {
        var targets = GetAllMarkedEntities();
        List<string> names = [];
        foreach (var uid in targets)
        {
            if(!EntMan.TryGetComponent<MetaDataComponent>(uid, out var metadata))
                continue;

            names.Add(metadata.EntityName);
        }

        if(names.Count == 1)
        {
            Announce(Loc.GetString(Objective.Announcement, ("names", names[0])));
            return;
        }

        var namesString = $"{string.Join("; ", names[..^1])} and {names[^1]}";
        Announce(Loc.GetString(Objective.Announcement, ("names", namesString)));
    }
    public override void BeforeFTLFromMap(EntityUid shuttle)
    {
        if (GetExpeditionConsole(shuttle) is not EntityUid expedConsole)
            return;

        var allTargets = GetAllMarkedEntitiesOnShuttle(shuttle);
        var numBonus = 0;
        foreach (var uid in allTargets)
            if(EntMan.TryGetComponent<DamageableComponent>(uid, out var damage) &&
               damage.TotalDamage <= TotalDamageForBonus)
                numBonus++;
        numBonus = Math.Min(numBonus, Objective.BonusCap);

        var completion = (allTargets.Count >= Objective.NumTargets.GetValueOrDefault(Difficulty, 0),
                          numBonus,
                          Objective.BonusCap,
                          allTargets.Count >= Objective.NumTargets.GetValueOrDefault(Difficulty, 0) ?
                            Objective.BaseReward.GetValueOrDefault(Difficulty, 0) + Objective.Bonus * numBonus :
                            0);
        SetRewardComponent(expedConsole, completion);
        DeleteWithEffect(allTargets);

    }
    public override void BeforeFTLToMap(EntityUid shuttle)
    {

    }
    public override void OnMapCreated()
    {
        var factions = IoCManager.Resolve<ISharedFactionManager>();
        var humanoid = EntMan.System<HumanoidAppearanceSystem>();
        var metadata = EntMan.System<MetaDataSystem>();
        var state = EntMan.System<MobStateSystem>();
        var damageable = EntMan.System<DamageableSystem>();
        var stationSpawning = EntMan.System<StationSpawningSystem>();
        var inventory = EntMan.System<InventorySystem>();

        var damageTypes = ProtoMan.EnumeratePrototypes<DamageTypePrototype>().ToList();

        int objectivesSpawned = 0;

        var possibleFactions = factions.ListPlayableFactions().Where(p => p.Major).ToList();
        var selectedFaction = possibleFactions[Rand.Next(possibleFactions.Count)];

        for (var i = 0; i < GetNumSpawnables(); i++)
        {
            var character = HumanoidCharacterProfile.Random();
            var species = ProtoMan.Index(character.Species);

            if (GetRandomEmptyTileInDungeon() is not EntityCoordinates pos)
                return;

            var ent = EntMan.SpawnAtPosition(species.Prototype, pos);
            humanoid.LoadProfile(ent, character);
            metadata.SetEntityName(ent, character.Name);
            state.ChangeMobState(ent, MobState.Dead);

            var damage = new DamageSpecifier();
            damage.DamageDict.Add(damageTypes[Rand.Next(damageTypes.Count)].ID, Rand.Next(300));
            if (Rand.Next(100) > 50)
                damage.DamageDict.TryAdd(damageTypes[Rand.Next(damageTypes.Count)].ID, Rand.Next(300));
            if (Rand.Next(100) > 75)
                damage.DamageDict.TryAdd(damageTypes[Rand.Next(damageTypes.Count)].ID, Rand.Next(300));
            damageable.TryChangeDamage(ent, damage);

            var jobs = factions.ListFactionJobs().Where(p => p.Faction == selectedFaction).ToList();
            var job = jobs[Rand.Next(jobs.Count)];
            var loadoutProtoId = factions.OverrideJobLoadout(job);
            var loadoutProto = ProtoMan.Index(loadoutProtoId);

            var loadout = new RoleLoadout(loadoutProtoId);
            loadout.SetDefault(character, null, ProtoMan);

            stationSpawning.EquipRoleLoadout(ent, loadout, loadoutProto);

            if (Rand.Prob(0.4))
            {
                var gasMaskEnt = EntMan.SpawnAtPosition(GasMask, pos);
                inventory.TryEquip(ent, gasMaskEnt, MaskSlot, force: true);
            }

            if (objectivesSpawned < Objective.NumTargets.GetValueOrDefault(Difficulty, 0))
            {
                MarkEntity(ent);
                objectivesSpawned++;
            }
        }
    }

    private int GetNumSpawnables() => 
        Objective.NumTargets.GetValueOrDefault(Difficulty, 0) + DecoyBodies;
}