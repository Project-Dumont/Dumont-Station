// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Client.Audio;

public sealed partial class ContentAudioSystem
{
    /// <summary>
    /// If we r holding the ambient music or not. <see cref="DisableAmbientMusic"/> only fade the music
    /// what is playing now, but next music come and play again, so this make all other music dont start playing.
    /// </summary>
    public bool AmbientMusicSuppressed { get; private set; }

    /// <summary>
    /// Keep ambient music quiet until you call this again with false. Dont forget call it again or music never come back. 
    /// and after 3 hours it worked lol
    /// </summary>
    public void SetAmbientMusicSuppressed(bool suppressed)
    {
        AmbientMusicSuppressed = suppressed;

        if (suppressed)
            DisableAmbientMusic();
    }
}
