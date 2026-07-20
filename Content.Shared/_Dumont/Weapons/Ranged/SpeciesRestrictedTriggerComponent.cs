using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Weapons.Ranged;

/// <summary>
/// Causes the weapon's trigger to break when fired by an entity possessing
/// one of the configured restricted species tags.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpeciesRestrictedTriggerComponent : Component
{
    /// <summary>
    /// Whether the trigger-breaking behavior is enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// Tags belonging to species that will break the trigger when attempting
    /// to fire this weapon.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<SpeciesPrototype>> RestrictedSpecies = [];

    /// <summary>
    /// Sound played when the trigger breaks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier BreakSound =
        new SoundPathSpecifier("/Audio/_Dumont/Misc/tool_break.ogg");

    /// <summary>
    /// Minimum delay, in seconds, between repeated failure feedback.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Last time the trigger-breaking popup was shown.
    /// Runtime state only.
    /// </summary>
    public TimeSpan LastPopup;
}
