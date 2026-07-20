// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
    public List<ProtoId<TagPrototype>> RestrictedSpecies = [];

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
    public float PopupCooldown = 1f;

    /// <summary>
    /// Last time the trigger-breaking popup was shown.
    /// Runtime state only.
    /// </summary>
    public TimeSpan LastPopup;
}
