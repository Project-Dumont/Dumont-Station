// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Cloning;

/// <summary>
/// Raised on the original body when its clone has a mind added (usually via the cloning EUI)
/// </summary>
[ByRefEvent]
public record struct TransferredToCloneEvent(EntityUid Cloned);
