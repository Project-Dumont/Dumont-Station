using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Robust.Shared.Map;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphDesignSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;

    public void Modifiers(EntityUid user, EntityCoordinates coordinates, out float time, out float cost)
    {
        time = cost = 1;
        if (!HasComp<XenomorphComponent>(user)) return;
        foreach (var node in _lookup.GetEntitiesInRange<XenomorphDesignNodeComponent>(coordinates, .4f, LookupFlags.All))
        {
            if (TerminatingOrDeleted(node) || TerminatingOrDeleted(node.Comp.Weed)) continue;
            if (node.Comp.Kind == XenomorphDesignType.Optimized) time = .5f;
            if (node.Comp.Kind == XenomorphDesignType.Flexible) cost = .5f;
        }
    }
}
