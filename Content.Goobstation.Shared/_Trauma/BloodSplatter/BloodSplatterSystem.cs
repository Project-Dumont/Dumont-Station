// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Shared.Coordinates;
using Content.Shared.Spawners.Components;
using Content.Shared.Throwing;
// Dumont end

namespace Content.Trauma.Shared.BloodSplatter;

public sealed partial class BloodSplatterSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodSplatterOnLandComponent, LandEvent>(OnLand);
    }

    private void OnLand(Entity<BloodSplatterOnLandComponent> ent, ref LandEvent args)
    {
        SpawnDecal(ent, ent.Comp.Color, ent.Comp.Decal);

        if (ent.Comp.DeleteEntity)
            PredictedQueueDel(ent);
    }

    private void SpawnDecal(EntityUid ent, Color color, string decal)
    {
        var spawnedDecal = EntityManager.CreateEntityUninitialized(decal, ent.ToCoordinates());

        if (TryComp<RandomDecalSpawnerComponent>(spawnedDecal, out var randomDecal))
        {
            randomDecal.Color = color;
        }

        EntityManager.InitializeAndStartEntity(spawnedDecal);
    }
}
// Dumont end
