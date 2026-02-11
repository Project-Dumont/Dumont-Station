// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT

namespace Content.Server._Gabystation.Jukebox;

[RegisterComponent]
public sealed partial class JukeboxMusicComponent : Component
{
    [DataField]
    public EntityUid Jukebox;
}