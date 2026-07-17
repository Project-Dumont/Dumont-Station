using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Dumont.Weapons.Ranged;

[RegisterComponent, NetworkedComponent]
public sealed partial class WeaponTriggerBrokenComponent : Component
{
    /// <summary>
    /// Sound played when the broken trigger is pulled.
    /// </summary>
    [DataField]
    public SoundSpecifier ClickSound =
        new SoundPathSpecifier("/Audio/Weapons/Guns/Empty/empty.ogg");

    /// <summary>
    /// Minimum time, in seconds, between firing-failure popups and sounds.
    /// </summary>
    [DataField]
    public float PopupCooldown = 1f;

    /// <summary>
    /// Time, in seconds, required to repair the trigger.
    /// </summary>
    [DataField]
    public float RepairDuration = 5f;

    /// <summary>
    /// Last time the firing-failure popup was shown.
    /// </summary>
    [ViewVariables]
    public TimeSpan LastPopupTime;
}
