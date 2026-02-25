using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class WegaHulkComponent : Component
{
    public readonly EntProtoId[] ActionPrototypes = new EntProtoId[]
    {
        "ActionHulkCharge"
    };

    public List<EntityUid?> ActionsEntity { get; set; } = new();
}

[RegisterComponent]
public sealed partial class WegaHulkGenComponent : Component
{
    public readonly EntProtoId ActionPrototype = "ActionHulkTransformation";

    public EntityUid? ActionEntity { get; set; }

    [DataField]
    public ProtoId<PolymorphPrototype> PolymorphProto = "HulkPolymorph";

    [DataField]
    public ProtoId<PolymorphPrototype> PolymorphAltProto = "HulkPolymorphAlt";
}
