using Content.Shared._CMU14.Xenomorphs;
using Content.Shared._White.Xenomorphs.Caste;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Xenomorphs;

[Prototype("xenomorphLineage")]
public sealed partial class XenomorphLineagePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>> Species = new();

    [DataField]
    public Dictionary<ProtoId<XenomorphCastePrototype>, List<EntProtoId>> Evolutions = new();
}
