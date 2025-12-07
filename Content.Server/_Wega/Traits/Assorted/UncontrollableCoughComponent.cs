using System.Numerics;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Traits.Assorted;

[RegisterComponent]
public sealed partial class UncontrollableCoughComponent : Component
{
    [DataField("emote", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EmotePrototype>))]
    public string EmoteId = string.Empty;

    [DataField("timeBetweenIncidents", required: true)]
    public Vector2 TimeBetweenIncidents { get; set; }

    public float NextIncidentTime;
}
