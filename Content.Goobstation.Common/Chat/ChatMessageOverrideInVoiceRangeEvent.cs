// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 JohnJohn <189290423+JohnJJohn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Chat;

[ByRefEvent]
public record struct ChatMessageOverrideInVoiceRange(bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}
