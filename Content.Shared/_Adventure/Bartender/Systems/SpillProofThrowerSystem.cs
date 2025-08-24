using Content.Shared.Inventory;
using Content.Shared._Adventure.Bartender.Components;

namespace Content.Shared._Adventure.Bartender.Systems;

public sealed class SpillProofThrowerSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.SubscribeWithRelay<SpillProofThrowerComponent, SpillProofThrowEvent>(OnSpillProofThrowAttempt);
        SubscribeLocalEvent<InventoryComponent, SpillProofThrowEvent>(_inventory.RelayEvent);
    }

    public bool GetSpillProofThrow(EntityUid playeruid)
    {
        var nonSpillThrowEvent = new SpillProofThrowEvent();
        RaiseLocalEvent(playeruid, ref nonSpillThrowEvent);
        return nonSpillThrowEvent.NonSpillThrow;
    }

    private void OnSpillProofThrowAttempt(Entity<SpillProofThrowerComponent> ent, ref SpillProofThrowEvent args)
    {
        args.NonSpillThrow = true;
    }
}

[ByRefEvent]
public record struct SpillProofThrowEvent(bool NonSpillThrow = false) : IInventoryRelayEvent
{
    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.HEAD | SlotFlags.MASK | SlotFlags.EYES;
}
