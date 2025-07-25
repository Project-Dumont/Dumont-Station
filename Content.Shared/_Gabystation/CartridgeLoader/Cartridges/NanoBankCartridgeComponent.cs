using Content.Shared._Gabystation.NanoBank;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.CartridgeLoader.Cartridges;

[RegisterComponent, NetworkedComponent]
public sealed partial class NanoBankCartridgeComponent : Component
{
    /// <summary>
    ///     The NanoBank card.
    /// </summary>
    [DataField]
    public EntityUid? Card;

    /*
    [DataField]
    public EntityUid? Station;*/
}
