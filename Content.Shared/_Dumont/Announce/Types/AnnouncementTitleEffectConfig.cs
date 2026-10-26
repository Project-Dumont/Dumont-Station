using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Announce;

[DataDefinition, Serializable, NetSerializable]
public sealed partial class AnnouncementTitleEffectConfig
{
    [DataField]
    public AnnouncementTitleEffectType Type { get; set; } = AnnouncementTitleEffectType.None;

    [DataField]
    public float Speed { get; set; } = 180f;

    [DataField]
    public float Gap { get; set; } = 48f;
}
