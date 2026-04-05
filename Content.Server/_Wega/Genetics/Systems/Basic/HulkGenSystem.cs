// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Destructible;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Genetics;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Stunnable;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Genetics.System;

public sealed class HulkGenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly DnaModifierSystem _dnaModifier = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    private static readonly ProtoId<StructuralEnzymesPrototype> HulkGen = "GeneticsHulkBasic";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WegaHulkGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<WegaHulkGenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<WegaHulkGenComponent, HulkTransformationActionEvent>(OnTransformation);

        SubscribeLocalEvent<WegaHulkComponent, ComponentInit>(OnHulkInit);
        SubscribeLocalEvent<WegaHulkComponent, ComponentShutdown>(OnHulkShutdown);
        SubscribeLocalEvent<WegaHulkComponent, HulkChargeActionEvent>(OnHulkCharge);
    }

    private void OnInit(Entity<WegaHulkGenComponent> ent, ref ComponentInit args)
        => ent.Comp.ActionEntity = _action.AddAction(ent, ent.Comp.ActionPrototype);

    private void OnShutdown(Entity<WegaHulkGenComponent> ent, ref ComponentShutdown args)
        => _action.RemoveAction(ent.Comp.ActionEntity);

    private void OnTransformation(Entity<WegaHulkGenComponent> ent, ref HulkTransformationActionEvent args)
    {
        args.Handled = true;
        if (!TryComp<DnaModifierComponent>(ent, out var dnaModifier) || dnaModifier.EnzymesPrototypes == null
            || !TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
            return;

        foreach (var enzymeInfo in dnaModifier.EnzymesPrototypes)
        {
            if (enzymeInfo.EnzymesPrototypeId == HulkGen)
            {
                enzymeInfo.HexCode = new[] { "3", "3", "3" };
                _dnaModifier.ChangeDna((ent, dnaModifier), 1);
                break;
            }
        }

        var polymorph = CheckSpeciesEntity(humanoid)
            ? ent.Comp.PolymorphAltProto
            : ent.Comp.PolymorphProto;

        _polymorph.PolymorphEntity(ent, polymorph);
    }

    private bool CheckSpeciesEntity(HumanoidAppearanceComponent humanoid)
    {
        var altSpecies = new[]
        {
            new ProtoId<SpeciesPrototype>("Reptilian"),
            new ProtoId<SpeciesPrototype>("Resomi"),
            new ProtoId<SpeciesPrototype>("Vox")
        };
        return altSpecies.Contains(humanoid.Species);
    }

    #region Abilities
    private void OnHulkInit(Entity<WegaHulkComponent> ent, ref ComponentInit args)
    {
        foreach (var action in ent.Comp.ActionPrototypes)
            ent.Comp.ActionsEntity.Add(_action.AddAction(ent, action));
    }

    private void OnHulkShutdown(Entity<WegaHulkComponent> ent, ref ComponentShutdown args)
    {
        foreach (var action in ent.Comp.ActionsEntity)
            _action.RemoveAction(action);
    }

    private void OnHulkCharge(Entity<WegaHulkComponent> entity, ref HulkChargeActionEvent args)
    {
        var target = args.Target;
        var vampirePosition = _transform.GetWorldPosition(entity);
        var targetPosition = _transform.ToMapCoordinates(Transform(target).Coordinates, true).Position;
        var direction = (targetPosition - vampirePosition).Normalized();

        if (TryComp(entity, out PhysicsComponent? vampirePhysics))
            _physics.ApplyLinearImpulse(entity, direction * 20000f, body: vampirePhysics);

        if (TryComp(target, out DestructibleComponent? _))
        {
            var damage = new DamageSpecifier { DamageDict = { { "Structural", 300 } } };
            _damage.TryChangeDamage(target, damage, origin: entity);
        }

        if (TryComp(target, out BodyComponent? _))
        {
            var damage = new DamageSpecifier { DamageDict = { { "Blunt", 60 } } };
            _damage.TryChangeDamage(target, damage, ignoreResistances: false, origin: entity);

            if (TryComp(target, out PhysicsComponent? physics))
                _physics.ApplyLinearImpulse(target, direction * 1000f, body: physics);

            _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(10f));
        }

        _audio.PlayPvs(args.Sound, entity);
        args.Handled = true;
    }
    #endregion
}
