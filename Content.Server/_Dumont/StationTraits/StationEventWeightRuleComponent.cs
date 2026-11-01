using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.StationTraits;
[RegisterComponent]
public sealed partial class StationEventWeightRuleComponent : Component
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, float> Modifiers = new();
}
