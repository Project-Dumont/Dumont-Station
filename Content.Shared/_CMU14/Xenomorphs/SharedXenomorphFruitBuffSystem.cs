using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphFruitBuffSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphFruitBuffComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphFruitBuffComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphFruitBuffComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnSpeed(Entity<XenomorphFruitBuffComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<MovementSpeedModifierComponent>(ent, out var speed)) return;
        args.ModifySpeed(1 + ent.Comp.SpeedBonus / Math.Max(0.01f, speed.BaseWalkSpeed),
            1 + ent.Comp.SpeedBonus / Math.Max(0.01f, speed.BaseSprintSpeed));
    }

    private void OnState(Entity<XenomorphFruitBuffComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);

    private void OnShutdown(Entity<XenomorphFruitBuffComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.SpeedBonus = 0;
        _movement.RefreshMovementSpeedModifiers(ent);
    }
}
