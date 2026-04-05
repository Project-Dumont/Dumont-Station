// SPDX-FileCopyrightText: 2026 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Mind;

/// <summary>
/// Raised on an entity to check if it should be excluded from objective target selection.
/// </summary>
[ByRefEvent]
public record struct GetAntagSelectionBlockerEvent(bool Blocked = false);
