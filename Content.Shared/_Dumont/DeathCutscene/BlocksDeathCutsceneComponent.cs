// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Dumont.DeathCutscene;

/// <summary>
/// Put this on a implant and whoever have it dont see the death cutscene. Its for stuff that
/// move the player somewhere else when he die, like bluespace lifeline, so the cutscene dont play just a sound with nothing showing.
/// finaly it works ahhhhh
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BlocksDeathCutsceneComponent : Component;
