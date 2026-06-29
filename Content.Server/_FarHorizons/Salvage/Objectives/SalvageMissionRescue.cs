using System.Linq;
using Content.Server.Humanoid;
using Content.Server.Station.Systems;
// Dumont
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Robust.Shared.Map;
using Robust.Shared.Prototypes; // Dumont
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
        var humanoid = EntMan.System<HumanoidAppearanceSystem>();
        var metadata = EntMan.System<MetaDataSystem>();
        var state = EntMan.System<MobStateSystem>();
        var damageable = EntMan.System<DamageableSystem>();
        var stationSpawning = EntMan.System<StationSpawningSystem>();
        var inventory = EntMan.System<InventorySystem>();

        var damageTypes = ProtoMan.EnumeratePrototypes<DamageTypePrototype>().ToList();

        int objectivesSpawned = 0;

        // Dumont
        string[] possibleJobs = { "Passenger", "MedicalDoctor", "SalvageSpecialist", "CargoTechnician", "Scientist" };

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

            // Dumont
            var chosenJobId = possibleJobs[Rand.Next(possibleJobs.Length)];
            
            if (ProtoMan.TryIndex<RoleLoadoutPrototype>(chosenJobId, out var loadoutProto))
            {
                var loadout = new RoleLoadout(chosenJobId);
                loadout.SetDefault(character, null, ProtoMan);
                stationSpawning.EquipRoleLoadout(ent, loadout, loadoutProto);
            }

            if (Rand.Prob(0.4))
            {
                var gasMaskEnt = EntMan.SpawnAtPosition(GasMask, pos);
                if (!inventory.TryEquip(ent, gasMaskEnt, MaskSlot, force: true))
                    EntMan.DeleteEntity(gasMaskEnt);
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