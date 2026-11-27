using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class MindCommunicationGenComponent : Component
{
    public readonly EntProtoId Action = "ActionMindCommunicationGen";

    public EntityUid? ActionEntity { get; set; }
}
