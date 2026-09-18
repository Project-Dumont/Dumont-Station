using System.Numerics;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Traits.Assorted;

[RegisterComponent]
public sealed partial class UncontrollableCoughComponent : Component
{
    [DataField("emote")]
    public ProtoId<EmotePrototype> EmoteId = "Cough";

    [DataField("timeBetweenIncidents")]
    public Vector2 TimeBetweenIncidents = new(30, 120);

    public float NextIncidentTime;
}
