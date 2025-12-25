using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Orion.Power.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InducerComponent : Component
{
    [DataField]
    public string PowerCellSlotId = "inducer_power_cell_slot";

    [DataField, AutoNetworkedField]
    public float TransferRate;

    [DataField]
    public List<float> AvailableTransferRates = new();

    [DataField]
    public float TransferDelay;

    /// <summary>
    ///     Multiply transferring energy.
    /// </summary>
    [DataField]
    public float TransferMultiplier;

    [DataField]
    public float MaxDistance;
}

[Serializable, NetSerializable]
public sealed partial class InducerDoAfterEvent : SimpleDoAfterEvent;
