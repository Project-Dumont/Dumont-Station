using System.Linq;
using Content.Server.Destructible;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
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

    [ValidatePrototypeId<StructuralEnzymesPrototype>]
    private const string HulkGen = "GeneticsHulkBasic";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HulkGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HulkGenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HulkGenComponent, HulkTransformationActionEvent>(OnTransformation);

        SubscribeLocalEvent<HulkComponent, ComponentInit>(OnHulkInit);
        SubscribeLocalEvent<HulkComponent, ComponentShutdown>(OnHulkShutdown);
        SubscribeLocalEvent<HulkComponent, HulkChargeActionEvent>(OnHulkCharge);
    }

    private void OnInit(Entity<HulkGenComponent> ent, ref ComponentInit args)
        => ent.Comp.ActionEntity = _action.AddAction(ent, ent.Comp.ActionPrototype);

    private void OnShutdown(Entity<HulkGenComponent> ent, ref ComponentShutdown args)
        => _action.RemoveAction(ent.Comp.ActionEntity);

    private void OnTransformation(Entity<HulkGenComponent> ent, ref HulkTransformationActionEvent args)
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
    private void OnHulkInit(Entity<HulkComponent> ent, ref ComponentInit args)
    {
        foreach (var action in ent.Comp.ActionPrototypes)
            ent.Comp.ActionsEntity.Add(_action.AddAction(ent, action));
    }

    private void OnHulkShutdown(Entity<HulkComponent> ent, ref ComponentShutdown args)
    {
        foreach (var action in ent.Comp.ActionsEntity)
            _action.RemoveAction(action);
    }

    private void OnHulkCharge(Entity<HulkComponent> entity, ref HulkChargeActionEvent args)
    {
        if (args.Coords is not { } coords)
            return;

        var vampirePosition = _transform.GetWorldPosition(entity);
        var targetPosition = _transform.ToMapCoordinates(coords, true).Position;
        var direction = (targetPosition - vampirePosition).Normalized();

        if (TryComp(entity, out PhysicsComponent? vampirePhysics))
            _physics.ApplyLinearImpulse(entity, direction * 20000f, body: vampirePhysics);

        if (args.Entity is not { } targetEntity)
        {
            _audio.PlayPvs(args.Sound, entity);
            args.Handled = true;
            return;
        }

        if (TryComp(targetEntity, out DestructibleComponent? _))
        {
            var damage = new DamageSpecifier { DamageDict = { { "Structural", 300 } } };
            _damage.TryChangeDamage(targetEntity, damage, origin: entity);
        }

        if (TryComp(targetEntity, out BodyComponent? _))
        {
            var damage = new DamageSpecifier { DamageDict = { { "Blunt", 60 } } };
            _damage.TryChangeDamage(targetEntity, damage, ignoreResistances: false, origin: entity);

            if (TryComp(targetEntity, out PhysicsComponent? physics))
                _physics.ApplyLinearImpulse(targetEntity, direction * 1000f, body: physics);

            _stun.TryParalyze(targetEntity, TimeSpan.FromSeconds(10f), true);
        }

        _audio.PlayPvs(args.Sound, entity);
        args.Handled = true;
    }
    #endregion
}