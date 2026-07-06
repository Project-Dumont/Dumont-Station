using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.ChemicalSpoilage;

[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public sealed partial class SpoiledReagentData : ReagentData
{
    [DataField]
    public ProtoId<ReagentPrototype> OriginalReagent;

    public SpoiledReagentData()
    {
    }

    public SpoiledReagentData(ProtoId<ReagentPrototype> originalReagent)
    {
        OriginalReagent = originalReagent;
    }

    public override ReagentData Clone()
    {
        return new SpoiledReagentData(OriginalReagent);
    }

    public override bool Equals(ReagentData? other)
    {
        return other is SpoiledReagentData data && data.OriginalReagent == OriginalReagent;
    }

    public override int GetHashCode()
    {
        return OriginalReagent.GetHashCode();
    }
}
