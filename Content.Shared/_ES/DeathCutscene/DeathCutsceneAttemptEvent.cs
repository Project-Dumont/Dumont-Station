// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._ES.DeathCutscene;

/// <summary>
/// This event happen on a mob before his death cutscene start, Cancel this if your system grab the player when
/// he die and you dont wanna him to see the cutscene, so it dont play for him.
/// nice
/// </summary>
[ByRefEvent]
public record struct DeathCutsceneAttemptEvent(bool Cancelled = false);
