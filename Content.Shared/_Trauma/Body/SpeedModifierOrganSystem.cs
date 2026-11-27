// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Movement.Systems;

namespace Content.Shared._Trauma.Body;

public sealed partial class SpeedModifierOrganSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeedModifierOrganComponent, BodyPartAddedEvent>(OnAttached);
        SubscribeLocalEvent<SpeedModifierOrganComponent, BodyPartRemovedEvent>(OnRemoved);
        SubscribeLocalEvent<SpeedModifierOrganBodyComponent, RefreshWeightlessModifiersEvent>(OnRefreshWeightless);
    }

    private void OnAttached(Entity<SpeedModifierOrganComponent> ent, ref BodyPartAddedEvent args)
    {
        if (args.Part.Comp.Body is not {} body)
            return;

        EnsureComp<SpeedModifierOrganBodyComponent>(body);
        _movement.RefreshWeightlessModifiers(body);
    }

    private void OnRemoved(Entity<SpeedModifierOrganComponent> ent, ref BodyPartRemovedEvent args)
    {
        if (args.Part.Comp.Body is not {} body || TerminatingOrDeleted(body))
            return;

        _movement.RefreshWeightlessModifiers(body);
    }

    private void OnRefreshWeightless(Entity<SpeedModifierOrganBodyComponent> ent, ref RefreshWeightlessModifiersEvent args)
    {
        foreach (var (part, comp) in _body.GetBodyChildren(ent.Owner))
        {
            if (!comp.Enabled || !TryComp<SpeedModifierOrganComponent>(part, out var modifier))
                continue;

            args.WeightlessAcceleration += modifier.WeightlessAcceleration;
            args.WeightlessModifier += modifier.WeightlessModifier;
            args.WeightlessFriction += modifier.WeightlessFriction;
        }
    }
}
