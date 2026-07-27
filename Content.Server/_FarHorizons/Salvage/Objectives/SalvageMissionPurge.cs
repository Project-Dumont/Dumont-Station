// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Collections.Generic;
using Content.Server.Humanoid;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._FarHorizons.Salvage.Objectives;

public sealed partial class SalvageMissionPurge : BaseSalvageMissionObjectiveHandler
{
    private readonly List<EntProtoId> _purgeTargets = ["FolderSalvageMissionObjective"];
    private readonly double _inPocketChance = 0.3;
    static readonly List<string> _pocketSlots = ["pocket1", "pocket2"];
    static readonly List<EntProtoId> _stuffProtos = ["Pen", "LuxuryPen", "Lighter", "CheapLighter", "FlippoLighter", "CigPackBlue", "CigPackRed", "CigPackBlack", "Cigar", "CigarGold"];

    public override void AFterFTLToMap(EntityUid shuttle) => 
        Announce(GetAnnouncement());
        
    public override void BeforeFTLFromMap(EntityUid shuttle)
    {
        if (GetExpeditionConsole(shuttle) is not EntityUid expedConsole)
            return;
        
        var allTargets = GetAllMarkedEntities();
        var targetsDestroyed = GetNumSpawnables() - allTargets.Count();
        SetRewardComponent(expedConsole, ResolveCompletion(targetsDestroyed));
    }
    
    public override void BeforeFTLToMap(EntityUid shuttle){} // Override intentionally left empty

    public override void OnMapCreated()
    {
        var humanoid = EntMan.System<HumanoidAppearanceSystem>();
        var metadata = EntMan.System<MetaDataSystem>();
        var state = EntMan.System<MobStateSystem>();
        var damageable = EntMan.System<DamageableSystem>();
        var inventory = EntMan.System<InventorySystem>();

        int bodiesSpawned = 0;

        // Dumont
        for (var i = 0; i < GetNumSpawnables(); i++)
        {
            if (GetRandomEmptyTileInDungeon() is not EntityCoordinates pos)
                continue;

            var proto = _purgeTargets[Rand.Next(_purgeTargets.Count)];
            var spawned = SpawnAndMarkEntity(proto, pos);

            // Dumont
            if (Rand.Prob(_inPocketChance))
            {
                var slot = _pocketSlots[Rand.Next(_pocketSlots.Count)];
                var damage = SalvageMissionRescue.RandomDamage(ProtoMan, Rand, 100, 200, 4);
                
                // Dumont
                var body = SalvageMissionRescue.SpawnRandomBody(ProtoMan, EntMan, Rand, pos, humanoid, metadata, inventory, state, damageable, true, damage, true);
                
                if (!inventory.TryEquip(body, spawned, slot, force: true))
                    EntMan.DeleteEntity(body);
                else
                {
                    bodiesSpawned++;
                }
            }
        }

        // Dumont
        for (var i = 0; i < bodiesSpawned * 2; i++)
        {
            if (GetRandomEmptyTileInDungeon() is not EntityCoordinates pos)
                continue;

            var slot = _pocketSlots[Rand.Next(_pocketSlots.Count)];
            var item = _stuffProtos[Rand.Next(_stuffProtos.Count)];

            var damage = SalvageMissionRescue.RandomDamage(ProtoMan, Rand, 100, 200, 4);
            
            // Dumont
            var body = SalvageMissionRescue.SpawnRandomBody(ProtoMan, EntMan, Rand, pos, humanoid, metadata, inventory, state, damageable, true, damage, true);
            
            var itemEnt = EntMan.SpawnAtPosition(item, pos);
            if (!inventory.TryEquip(body, itemEnt, slot, force: true))
                EntMan.DeleteEntity(itemEnt);
        }
    }

    private int GetNumSpawnables() => 
        Objective.NumTargets.GetValueOrDefault(Difficulty, 0) + Objective.BonusCap;
}