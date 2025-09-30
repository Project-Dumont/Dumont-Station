using Content.Shared._DV.CartridgeLoader.Cartridges;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Gabystation.NanoBank;

[RegisterComponent, NetworkedComponent]
public sealed partial class NanoBankCardComponent : Component
{
    [DataField]
    public bool LoggedIn = false;

    [DataField]
    public int AccountId = 0;

    [DataField]
    public int AccountPin = 0;

    [DataField]
    public bool NotificationsMuted = false;

    /// <summary>
    /// The station linked to the account.
    /// </summary>
    [DataField]
    public EntityUid? Station;
}
