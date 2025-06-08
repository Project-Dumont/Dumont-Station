using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation.CartridgeLoader.Cartridges;

[RegisterComponent, Access(typeof(NanoBankCartridgeSystem))]
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
