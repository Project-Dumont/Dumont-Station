using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class MindCommunicationGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public readonly string Action = "ActionMindCommunicationGen";

    public EntityUid? ActionEntity { get; set; }
}
