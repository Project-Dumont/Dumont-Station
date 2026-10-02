// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Dumont.DeathCutscene;

/// <summary>
/// Put this on an implant and whoever has it won't see the death cutscene. Its for stuff that
/// moves the player somewhere else when he dies, like the bluespace lifeline. So, the cutscene won't play, there'll be just a sound with nothing showing.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BlocksDeathCutsceneComponent : Component;
