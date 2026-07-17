// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Emoting.Components;

/// <summary>
/// Component placed on species that have a tail, allowing them to use tail-specific duo emotes
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TailDuoEmoteComponent : Component
{
}
