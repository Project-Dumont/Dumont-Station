using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared.Andromeda.TextToSpeech;

[Serializable, NetSerializable]
public sealed class AnnounceTtsEvent
    : EntityEventArgs
{
    public byte[] Data { get; set; } = [];
    public SoundSpecifier? AnnouncementSound { get; set; }
}
