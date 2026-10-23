using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.DeepFryer;

[Prototype("deepFryerRecipe")]
[DataDefinition]
public sealed partial class DeepFryerRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = "";

    [DataField]
    public string Group = "Other";

    [DataField]
    public uint Time = 15;

    [DataField]
    public EntProtoId Result;

    [DataField]
    public Dictionary<EntProtoId, int> Ingredients = new();

    [DataField("reagents")]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> Reagents = new();
}