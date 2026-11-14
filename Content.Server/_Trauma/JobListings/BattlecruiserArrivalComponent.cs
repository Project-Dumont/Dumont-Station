// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Trauma.Server.JobListings;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class BattlecruiserArrivalComponent : Component
{
    [DataField(required: true)]
    public List<BattlecruiserAnnouncement> Announcements = new();

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? StartedAt;

    [DataField]
    public int NextAnnouncement;

    [DataField]
    public EntityUid? Music;
}

[DataDefinition]
public sealed partial class BattlecruiserAnnouncement
{
    [DataField]
    public TimeSpan Offset;

    [DataField]
    public LocId? Message;

    [DataField]
    public LocId Sender = "battlecruiser-announcement-sender";

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public SoundSpecifier? Music;
}
