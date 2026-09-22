using Content.Shared.PDA;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class JaniCartridgeComponent : Component
{
    /// <summary>
    /// Jani notification group
    /// </summary>
    [DataField]
    public ProtoId<NotificationGroupPrototype> NotificationGroup = "JanitorAlerts";

    /// <summary>
    /// Localized Message that appears on Jani's PDAs;
    /// </summary>
    [DataField]
    public LocId LocalizedMessage = "jani-cartridge-message";

    /// <summary>
    /// Determines the amount of time to wait between calling the Janitor.
    /// Made in order to prevent abuse.
    /// </summary>
    [DataField]
    public TimeSpan CallDelay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Last used time, used for delay;
    /// </summary>
    [DataField]
    public TimeSpan LastUsed = TimeSpan.Zero;
}
