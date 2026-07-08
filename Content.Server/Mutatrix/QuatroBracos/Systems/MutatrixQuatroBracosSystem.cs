// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Destructible;
using Content.Server.Mutatrix.QuatroBracos.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mutatrix.QuatroBracos.Events;
using Content.Shared.Stunnable;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;

namespace Content.Server.Mutatrix.QuatroBracos.Systems;

public sealed class MutatrixQuatroBracosSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MutatrixQuatroBracosComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MutatrixQuatroBracosComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MutatrixQuatroBracosComponent, MutatrixQuatroBracosKickActionEvent>(OnKick);
    }

    private void OnInit(Entity<MutatrixQuatroBracosComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent, ref ent.Comp.KickActionEntity, ent.Comp.KickAction);
    }

    private void OnShutdown(Entity<MutatrixQuatroBracosComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.KickActionEntity);
    }

    private void OnKick(Entity<MutatrixQuatroBracosComponent> ent, ref MutatrixQuatroBracosKickActionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;
        var originPosition = _transform.GetWorldPosition(ent);
        var targetPosition = _transform.ToMapCoordinates(Transform(target).Coordinates, true).Position;
        var direction = targetPosition - originPosition;
        if (direction.LengthSquared() == 0)
            return;

        direction = direction.Normalized();

        if (TryComp(target, out DestructibleComponent? _))
        {
            var structural = new DamageSpecifier { DamageDict = { { "Structural", ent.Comp.StructuralDamage } } };
            _damage.TryChangeDamage(target, structural, origin: ent);
        }

        if (TryComp(target, out BodyComponent? _))
        {
            var blunt = new DamageSpecifier { DamageDict = { { "Blunt", ent.Comp.BluntDamage } } };
            _damage.TryChangeDamage(target, blunt, ignoreResistances: false, origin: ent);

            if (TryComp(target, out PhysicsComponent? physics))
                _physics.ApplyLinearImpulse(target, direction * ent.Comp.KnockbackImpulse, body: physics);

            _stun.TryKnockdown(target, ent.Comp.KnockdownTime, true);
        }

        _audio.PlayPvs(ent.Comp.KickSound, ent);
        args.Handled = true;
    }
}
