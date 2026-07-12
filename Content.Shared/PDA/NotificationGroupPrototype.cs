using Robust.Shared.Prototypes;
using Content.Shared.Access;

namespace Content.Shared.PDA;

[Prototype]
public sealed class NotificationGroupPrototype : IPrototype {
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public HashSet<ProtoId<AccessGroupPrototype>>? AcessGroups = null;

    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>>? Access = null;

}
