// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Trauma.Body.Part;

public sealed partial class PullerTailSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private EntityQuery<HumanoidAppearanceComponent> _humanoidQuery = default!;
    [Dependency] private EntityQuery<PullerComponent> _pullerQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PullerTailComponent, BodyPartAddedEvent>(OnAttached);
        SubscribeLocalEvent<PullerTailComponent, BodyPartRemovedEvent>(OnRemoved);
    }

    private void OnAttached(Entity<PullerTailComponent> ent, ref BodyPartAddedEvent args)
    {
        if (_timing.ApplyingState ||
            args.Part.Comp.Body is not {} body ||
            !_pullerQuery.TryComp(body, out var puller) ||
            !puller.NeedsHands)
            return;

        if (ent.Comp.SpeciesWhitelist is {} whitelist &&
            !(_humanoidQuery.TryComp(body, out var humanoid) &&
            whitelist.Contains(humanoid.Species)))
            return;

        puller.NeedsHands = false;
        Dirty(body, puller);
        ent.Comp.Changed = true;
        Dirty(ent);
    }

    private void OnRemoved(Entity<PullerTailComponent> ent, ref BodyPartRemovedEvent args)
    {
        if (!ent.Comp.Changed || _timing.ApplyingState)
            return;

        ent.Comp.Changed = false;
        Dirty(ent);

        if (args.Part.Comp.Body is not {} body ||
            TerminatingOrDeleted(body) ||
            !_pullerQuery.TryComp(body, out var puller))
            return;

        puller.NeedsHands = true;
        Dirty(body, puller);
    }
}
