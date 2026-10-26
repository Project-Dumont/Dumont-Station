using Content.Shared.Atmos;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class CMUTileFireComponent : Component;

/// <summary>Adapts CMU's extinguishable floor fire to the station's reagent reactions.</summary>
public sealed partial class CMUTileFireSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CMUTileFireComponent, ExtinguishEvent>(OnExtinguish);
    }

    private void OnExtinguish(Entity<CMUTileFireComponent> ent, ref ExtinguishEvent args)
    {
        if (args.FireStacksAdjustment < 0) QueueDel(ent);
    }
}
