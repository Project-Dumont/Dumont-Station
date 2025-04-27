namespace Content.Shared.WashingMachine.Events;

public sealed partial class WashingMachineFinishedWashingEvent : EntityEventArgs
{
    public HashSet<EntityUid> Items;

    public WashingMachineFinishedWashingEvent(HashSet<EntityUid> items)
    {
        Items = items;
    }
}
