// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Mutatrix.Components;

/// <summary>
/// Marks a body created by Mutatrix polymorph, so the server can revert it if
/// the player leaves the body, ghosts, disconnects, or gets detached from it.
/// Server-only component. Do not Dirty().
/// </summary>
[RegisterComponent]
public sealed partial class MutatrixTransformedComponent : Component
{
}
