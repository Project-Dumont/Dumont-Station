using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Andromeda.TextToSpeech;

[RegisterComponent, NetworkedComponent]
public sealed partial class TextToSpeechComponent : Component
{
    [DataField("voice", customTypeSerializer: typeof(PrototypeIdSerializer<VoicePrototype>))]
    public string? VoicePrototypeId { get; set; }
}
