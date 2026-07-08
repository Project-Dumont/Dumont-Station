// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared.Mutatrix.Events;

/// <summary>
/// Raised by the Mutatrix action to request opening the radial transformation menu.
/// </summary>
public sealed partial class MutatrixOpenMenuActionEvent : InstantActionEvent;


/// <summary>
/// Raised by the Mutatrix capture action after the player chooses an entity.
/// </summary>
public sealed partial class MutatrixCaptureDnaActionEvent : EntityTargetActionEvent;
