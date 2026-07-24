// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using Content.Server.Humanoid;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Content.Shared.Roles; 
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
    
    public override void BeforeFTLToMap(EntityUid shuttle){} // Override intentionally left empty
    
    public override void OnMapCreated()
    {
        var humanoid = EntMan.System<HumanoidAppearanceSystem>();
        var metadata = EntMan.System<MetaDataSystem>();
        var state = EntMan.System<MobStateSystem>();
        var damageable = EntMan.System<DamageableSystem>();
        var inventory = EntMan.System<InventorySystem>();

        int objectivesSpawned = 0;

        for (var i = 0; i < GetNumSpawnables(); i++)
        {
            if (GetRandomEmptyTileInDungeon() is not EntityCoordinates pos)
                continue;

            var damage = RandomDamage(ProtoMan, Rand, 100, 200, 4);
            var body = SpawnRandomBody(ProtoMan, EntMan, Rand, pos, humanoid, metadata, inventory, state, damageable, true, damage, true);

            if (Rand.Prob(0.4))
            {
                var gasMaskEnt = EntMan.SpawnAtPosition(GasMask, pos);
                if (!inventory.TryEquip(body, gasMaskEnt, MaskSlot, force: true))
                    EntMan.DeleteEntity(gasMaskEnt);
            }

            if (objectivesSpawned < Objective.NumTargets.GetValueOrDefault(Difficulty, 0))
            {
                MarkEntity(body);
                objectivesSpawned++;
            }
        }
    }

    public static EntityUid SpawnRandomBody(
        IPrototypeManager ProtoMan,
        IEntityManager EntMan,
        Random Rand,
        EntityCoordinates pos, 
        HumanoidAppearanceSystem humanoidAppearance, 
        MetaDataSystem metadata,
        InventorySystem inventory,
        MobStateSystem? state = null,
        DamageableSystem? damageable = null,
        bool dead = true,
        DamageSpecifier? damage = null,
        bool randomLoadout = true)
    {
        var character = HumanoidCharacterProfile.Random();
        var species = ProtoMan.Index(character.Species);

        var ent = EntMan.SpawnAtPosition(species.Prototype, pos);
        humanoidAppearance.LoadProfile(ent, character);
        metadata.SetEntityName(ent, character.Name);

        if (dead && state != null)
            state.ChangeMobState(ent, MobState.Dead);
        
        if (damage != null && damageable != null)
            damageable.TryChangeDamage(ent, damage);
        
        if (randomLoadout && inventory != null)
        {
            // Dumont
            var jobProtos = ProtoMan.EnumeratePrototypes<JobPrototype>()
                .Where(j => j.SetPreference && 
                            !j.ID.ToLower().Contains("centcom") && 
                            !j.ID.ToLower().Contains("ert") && 
                            !j.ID.ToLower().Contains("admin") &&
                            !j.ID.ToLower().Contains("syndicate") &&
                            !j.ID.ToLower().Contains("pirate"))
                .ToList();

            if (jobProtos.Count > 0)
            {
                var job = jobProtos[Rand.Next(jobProtos.Count)];
                
                // Dumont
                if (job.StartingGear != null && ProtoMan.TryIndex<StartingGearPrototype>(job.StartingGear, out var gear))
                {
                    string[] allowedSlots = { "jumpsuit", "shoes", "outerClothing", "gloves" };

                    foreach (var (slot, itemProto) in gear.Equipment)
                    {
                        if (!allowedSlots.Contains(slot))
                            continue;

                        // Dumont
                        var item = EntMan.SpawnEntity(itemProto, pos);
                        var itemMeta = EntMan.GetComponent<MetaDataComponent>(item);
                        var realProtoName = itemMeta.EntityPrototype?.ID.ToLower() ?? "";

                        // Dumont
                        if (realProtoName.Contains("armor") || realProtoName.Contains("armour") || 
                            realProtoName.Contains("helmet") || realProtoName.Contains("hardsuit") || 
                            realProtoName.Contains("vest") || realProtoName.Contains("shield") ||
                            realProtoName.Contains("carapace") || realProtoName.Contains("rig") ||
                            realProtoName.Contains("voidsuit") || realProtoName.Contains("spacesuit"))
                        {
                            EntMan.DeleteEntity(item);
                            continue;
                        }
                        
                        // Dumont
                        if (!inventory.TryEquip(ent, item, slot, force: true))
                            EntMan.DeleteEntity(item);
                    }
                }
            }
        }

        return ent;
    }

    public static DamageSpecifier RandomDamage(IPrototypeManager ProtoMan, Random Rand, int minDamage, int maxDamage, int maxDamageTypes)
    {
        var damageTypes = ProtoMan.EnumeratePrototypes<DamageTypePrototype>().ToList();

        var damage = new DamageSpecifier();
        float chance = 1;
        for (var i = 0; i < maxDamageTypes; i++)
        {
            if(Rand.Prob(chance))
            {
                var type = damageTypes[Rand.Next(damageTypes.Count)].ID;
                if (damage.DamageDict.ContainsKey(type))
                    continue;

                damage.DamageDict.Add(type, Rand.Next(minDamage, maxDamage));       
            }
            chance -= 1 / maxDamageTypes;
        }

        return damage;
    }

    private int GetNumSpawnables() => 
        Objective.NumTargets.GetValueOrDefault(Difficulty, 0) + DecoyBodies;
}