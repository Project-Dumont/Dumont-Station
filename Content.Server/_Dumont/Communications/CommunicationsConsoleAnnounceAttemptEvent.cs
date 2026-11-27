// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Dumont.Communications;

[ByRefEvent]
public record struct CommunicationsConsoleAnnounceAttemptEvent(EntityUid Console, bool Cancelled = false);
