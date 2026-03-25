using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DragonStation.FighterProgression.Prototypes;

[DataDefinition]
[Prototype("fighterBranch")]
public sealed partial class FighterBranchPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name = string.Empty;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public Color Color = Color.White;
}
