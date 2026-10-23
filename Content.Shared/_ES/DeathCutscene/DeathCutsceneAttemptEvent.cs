// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._ES.DeathCutscene;

/// <summary>
/// Raised on a mob before his death cutscene starts, Cancel this if your system wants to stop the death cutscene from playing
/// </summary>
[ByRefEvent]
public record struct DeathCutsceneAttemptEvent(bool Cancelled = false);
