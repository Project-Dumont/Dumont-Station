using Content.Shared._CMU14.Xenomorphs;
using Content.Shared.Interaction;
using Content.Shared.Shuttles.Components;

namespace Content.Shared._Dumont.Xenomorphs;

/// <summary>Lets an immobile Queen reach the shuttle console from her ovipositor.</summary>
public sealed partial class XenomorphQueenConsoleInteractionSystem : EntitySystem
{
    public const float ConsoleRange = 3.5f;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphOvipositorComponent, InRangeOverrideEvent>(OnRange);
    }

    private void OnRange(Entity<XenomorphOvipositorComponent> ent, ref InRangeOverrideEvent args)
    {
        if (args.Handled || !ent.Comp.Attached || !HasComp<XenomorphRoyalComponent>(ent)
            || !_ui.HasUi(args.Target, ShuttleConsoleUiKey.Key)) return;
        var target = args.Target;
        args.Handled = true;
        args.InRange = _interaction.InRangeUnobstructed(
            _transform.GetMapCoordinates(ent), _transform.GetMapCoordinates(args.Target), ConsoleRange,
            predicate: uid => uid == ent.Owner || uid == target);
    }
}
