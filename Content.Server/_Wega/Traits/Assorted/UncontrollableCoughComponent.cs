using System.Numerics;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Traits.Assorted;

[RegisterComponent]
public sealed partial class UncontrollableCoughComponent : Component
{
    [DataField("emote", customTypeSerializer: typeof(PrototypeIdSerializer<EmotePrototype>))]
    public string EmoteId = "Cough";

    [DataField("timeBetweenIncidents")]
    public Vector2 TimeBetweenIncidents = new(30, 120);

    public float NextIncidentTime;
}
