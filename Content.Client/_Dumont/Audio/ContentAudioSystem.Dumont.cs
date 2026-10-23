// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Client.Audio;

public sealed partial class ContentAudioSystem
{
    /// <summary>
    /// If we are holding the ambient music or not. <see cref="DisableAmbientMusic"/> only fades the music
    /// that is playing now. The next music comes and plays again, so this stops them from playing.
    /// </summary>
    public bool AmbientMusicSuppressed { get; private set; }

    /// <summary>
    /// Keeps ambient music quiet until called again with false. Don't forget to call it again or the music will never come back. 
    /// </summary>
    public void SetAmbientMusicSuppressed(bool suppressed)
    {
        AmbientMusicSuppressed = suppressed;

        if (suppressed)
            DisableAmbientMusic();
    }
}
