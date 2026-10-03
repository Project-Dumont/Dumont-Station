// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Trauma.Shared.Genetics.Abilities;
using Content.Trauma.Shared.Genetics.Mutations;
using Content.Trauma.Shared.Popups;
using Content.Server.Polymorph.Systems;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Server.Genetics.Abilities;

public sealed partial class PolymorphMutationSystem : EntitySystem
{
    [Dependency] private PolymorphSystem _polymorph = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private TraumaPopupSystem _popup = default!;
    [Dependency] private EntityQuery<HumanoidAppearanceComponent> _humanoidQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PolymorphMutationComponent, MutationAddedEvent>(OnMutationAdded);
        SubscribeLocalEvent<PolymorphMutationComponent, MutationRemovedEvent>(OnMutationRemoved);
    }

    private void OnMutationAdded(Entity<PolymorphMutationComponent> ent, ref MutationAddedEvent args)
    {
        // polymorph automatically moves mutations so do nothing or it would be in some kind of hell
        if (args.Automatic)
            return;

        var target = args.Target.Owner;
        if (!_humanoidQuery.TryComp(target, out var humanoid))
            return;

        if (!ent.Comp.Prototypes.TryGetValue(humanoid.Species, out var proto))
        {
            Avisar(target, args.User, humanoid.Species);
            return;
        }

        if (_polymorph.PolymorphEntity(target, proto) == null)
            return;

        ent.Comp.Worked = true;
    }

    private void OnMutationRemoved(Entity<PolymorphMutationComponent> ent, ref MutationRemovedEvent args)
    {
        if (args.Automatic)
            return;

        var target = args.Target.Owner;
        if (ent.Comp.Worked)
        {
            _polymorph.Revert(target);
            return;
        }

        if (!_humanoidQuery.TryComp(target, out var humanoid))
            return;

        if (ent.Comp.Reverts.TryGetValue(humanoid.Species, out var revert))
        {
            _polymorph.PolymorphEntity(target, revert);
            return;
        }

        if (ent.Comp.Incompatible.Contains(humanoid.Species))
            return;

        if (ent.Comp.Fallback is {} fallback)
            _polymorph.PolymorphEntity(target, fallback);
    }

    private void Avisar(EntityUid target, EntityUid? user, ProtoId<SpeciesPrototype> species)
    {
        var nome = _proto.TryIndex(species, out var prototype)
            ? Loc.GetString(prototype.Name)
            : species.Id;

        _popup.PopupEntity(Loc.GetString("mutation-polymorph-incompatible", ("species", nome)),
            target, user ?? target);
    }
}
