// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mutatrix.Prototypes;
using Content.Shared.Mutatrix.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mutatrix.Components;

/// <summary>
/// Persistent Mutatrix DNA database for a character.
///
/// Static DNA uses MutatrixTransformationPrototype IDs for the 10 built-in forms.
/// Dynamic scan DNA uses entity prototype IDs, so the analyzer can remember any
/// scanned mob/species prototype until the round ends without needing a YAML
/// entry for every mob in the codebase.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMutatrixSystem))]
public sealed partial class MutatrixDnaComponent : Component
{
    /// <summary>
    /// Permanent/default species signatures available to this character.
    /// Default unlocked DNA is stored here.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<MutatrixTransformationPrototype>> Unlocked = new();

    /// <summary>
    /// Static prototype DNA captured during the current round.
    /// Kept for compatibility with older Mutatrix patches.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<MutatrixTransformationPrototype>> RoundUnlocked = new();

    /// <summary>
    /// Dynamic entity prototype DNA captured during this round.
    /// Example: MobHuman, MobMonkey, MobCat, MobXenomorphDrone, etc.
    /// These are cleared naturally on round restart because the player entity is
    /// recreated and the Mutatrix only rebuilds the static default DNA.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<string> RoundScannedPrototypes = new();

    /// <summary>
    /// Last selected static transformation.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<MutatrixTransformationPrototype>? Selected;

    /// <summary>
    /// Last selected dynamic scanned entity prototype.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? SelectedScannedPrototype;
}
