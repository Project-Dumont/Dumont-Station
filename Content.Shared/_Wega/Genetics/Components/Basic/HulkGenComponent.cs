using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class HulkComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public readonly string[] ActionPrototypes = new[]
    {
        "ActionHulkCharge"
    };

    public List<EntityUid?> ActionsEntity { get; set; } = new();
}

[RegisterComponent]
public sealed partial class HulkGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public readonly string ActionPrototype = "ActionHulkTransformation";

    public EntityUid? ActionEntity { get; set; }

    [DataField, ValidatePrototypeId<PolymorphPrototype>]
    public string PolymorphProto = "HulkPolymorph";

    [DataField, ValidatePrototypeId<PolymorphPrototype>]
    public string PolymorphAltProto = "HulkPolymorphAlt";
}
