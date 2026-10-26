using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphLineageComponent : Component
{
    [DataField]
    public ProtoId<XenomorphLineagePrototype> Lineage = "Human";
}
