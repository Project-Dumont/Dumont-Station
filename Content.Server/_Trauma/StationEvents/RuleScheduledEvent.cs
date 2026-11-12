// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Server.StationEvents;

/// <summary>
/// Event raised on an event scheduler gamerule after it starts a random gamerule.
/// </summary>
[ByRefEvent]
public record struct RuleScheduledEvent();
