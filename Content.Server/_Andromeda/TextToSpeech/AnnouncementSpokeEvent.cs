// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 cosmosgc <cosmoskitsune@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.Andromeda.TTS;

public sealed class AnnouncementSpokeEvent : EntityEventArgs
{
    public Filter Source { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? AnnounceVoice { get; set; } = null!;
    public SoundSpecifier? AnnouncementSound { get; set; } = null!;
}
