using Content.Shared.Genetics;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server.Genetics.System;

public sealed class SpeedUpGenSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeedUpGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SpeedUpGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<SpeedUpGenComponent> ent, ref ComponentInit args)
    {
        if (TryComp<MovementSpeedModifierComponent>(ent, out var speed))
        {
            var originalWalkSpeed = speed.BaseWalkSpeed;
            var originalSprintSpeed = speed.BaseSprintSpeed;
            _speed.ChangeBaseSpeed(ent, originalWalkSpeed * ent.Comp.SpeedModifier, originalSprintSpeed * ent.Comp.SpeedModifier, speed.Acceleration, speed);
        }
    }

    private void OnShutdown(Entity<SpeedUpGenComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<MovementSpeedModifierComponent>(ent, out var speed))
        {
            var originalWalkSpeed = speed.BaseWalkSpeed;
            var originalSprintSpeed = speed.BaseSprintSpeed;
            _speed.ChangeBaseSpeed(ent, originalWalkSpeed / ent.Comp.SpeedModifier, originalSprintSpeed / ent.Comp.SpeedModifier, speed.Acceleration, speed);
        }
    }
}
