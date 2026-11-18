// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Dumont.Niver;

/// <summary>
/// Greets the station at round start, then plays the birthday song once at a random time.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class NiverRuleComponent : Component
{
    [DataField]
    public LocId GreetAnnouncement = "niver-announcement-greet";

    [DataField]
    public LocId SongAnnouncement = "niver-announcement-song";

    [DataField]
    public SoundSpecifier Song = new SoundPathSpecifier("/Audio/_Dumont/Jukebox/parabens_da_xuxa.ogg");

    [DataField]
    public TimeSpan GreetDelay = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan MinSongDelay = TimeSpan.FromMinutes(10);

    [DataField]
    public TimeSpan MaxSongDelay = TimeSpan.FromMinutes(40);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? GreetTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? SongTime;
}
