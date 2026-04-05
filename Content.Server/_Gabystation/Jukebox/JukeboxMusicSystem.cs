// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT

using Content.Shared._Gabystation.Jukebox;
using Content.Shared.Audio.Jukebox;

namespace Content.Server._Gabystation.Jukebox;

public sealed class JukeboxMusicSystem : SharedJukeboxMusicSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JukeboxMusicComponent, ComponentRemove>(OnShutdown);
    }

    private void OnShutdown(Entity<JukeboxMusicComponent> music, ref ComponentRemove args)
    {
        if (!TryComp<JukeboxComponent>(music.Comp.Jukebox, out var jukebox))
            return;

        if (jukebox.AudioStream == music.Owner)
        {
            jukebox.AudioStream = null;
            Dirty(music.Comp.Jukebox, jukebox);
        }
    }
}