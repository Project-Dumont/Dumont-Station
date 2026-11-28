using Content.Shared.Inventory;

namespace Content.Shared.Temperature;

/// <summary>
/// This event is raised before heat is exchanged so that the conductance of the exchange can be changed.
/// </summary>
[ByRefEvent]
public record struct BeforeHeatExchangeEvent() : IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = ~SlotFlags.POCKET;
    // <Trauma>
    public EntityUid Target;
    public float OurTemp;
    public float OtherTemp;
    public bool Cancelled;
    // </Trauma>

    /// <summary>
    /// A multiplicative modifier for heat transfers on the entity this event is being raised to.
    /// </summary>
    public float HeatTransferModifier = 1f;
}
