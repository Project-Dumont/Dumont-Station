// SPDX-FileCopyrightText: 2025 MarkerWicker (OmuStation)
// SPDX-License-Identifier: AGPL-3.0-or-later
// Ported from OmuStation PRs #150 and #1613; adapted for Dumont.

using Content.Shared._Omu.Clothing.EntitySystems;
using Content.Shared.Speech;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Omu.Traits;

/// <summary>
///     This component indicates that a player is a Cybernetic Beast.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CyberneticMantleSystem), typeof(CyberneticBeastSystem))]
public sealed partial class CyberneticBeastComponent : Component
{

    public ProtoId<SpeechSoundsPrototype> SpeechSoundWhileWearingMantle = "Pai";

}
