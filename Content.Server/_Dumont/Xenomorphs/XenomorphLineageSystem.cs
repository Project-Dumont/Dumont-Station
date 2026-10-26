using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Linq;
using Content.Server._White.Xenomorphs.Evolution;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.RadialSelector;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphLineageSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = default!;

    public ProtoId<XenomorphLineagePrototype> GetLineage(EntityUid host)
    {
        if (TryComp<HumanoidAppearanceComponent>(host, out var appearance))
        {
            foreach (var lineage in _prototypes.EnumeratePrototypes<XenomorphLineagePrototype>())
            {
                if (lineage.Species.Contains(appearance.Species))
                    return lineage.ID;
            }
        }

        return "Human";
    }

    public List<RadialSelectorEntry> GetEvolutions(EntityUid uid, XenomorphEvolutionComponent evolution)
    {
        if (!TryComp<XenomorphLineageComponent>(uid, out var lineage))
            return evolution.EvolvesTo;

        if (!TryComp<XenomorphComponent>(uid, out var xeno))
            return new();

        if (_prototypes.Index(xeno.Caste).Tier == 3)
            return new() { new RadialSelectorEntry { Prototype = "MobXenomorphCMUQueen" } };

        if (!_prototypes.Index(lineage.Lineage).Evolutions.TryGetValue(xeno.Caste, out var choices))
            return new();

        return choices.Select(choice => new RadialSelectorEntry { Prototype = choice.Id }).ToList();
    }

    public bool CanEvolve(EntityUid uid, string? target)
    {
        if (!HasComp<XenomorphLineageComponent>(uid))
            return true;

        return TryComp<XenomorphEvolutionComponent>(uid, out var evolution)
            && GetEvolutions(uid, evolution).Any(choice => choice.Prototype == target);
    }

    public void Transfer(EntityUid source, EntityUid target)
    {
        if (TryComp<XenomorphLineageComponent>(source, out var lineage))
            EnsureComp<XenomorphLineageComponent>(target).Lineage = lineage.Lineage;
    }
}
