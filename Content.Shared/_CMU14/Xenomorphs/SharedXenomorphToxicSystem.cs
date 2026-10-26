using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Movement.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphToxicSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphToxicComponent, RefreshMovementSpeedModifiersEvent>(OnToxicSpeed);
        SubscribeLocalEvent<XenomorphToxicComponent, AfterAutoHandleStateEvent>(OnToxicState);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, RefreshMovementSpeedModifiersEvent>(OnIntoxicatedSpeed);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, AfterAutoHandleStateEvent>(OnIntoxicatedState);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, ComponentShutdown>(OnIntoxicatedShutdown);
    }
    public static float Speed(int stacks) => stacks < 20 ? 1 : .85f - .35f * Math.Clamp((stacks - 20) / 10f, 0, 1);
    private void OnToxicSpeed(Entity<XenomorphToxicComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.SpeedUntil > _timing.CurTime) args.ModifySpeed(1.2f, 1.2f);
    }
    private void OnIntoxicatedSpeed(Entity<XenomorphIntoxicatedComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
        => args.ModifySpeed(Speed(ent.Comp.Stacks), Speed(ent.Comp.Stacks));
    private void OnToxicState(Entity<XenomorphToxicComponent> ent, ref AfterAutoHandleStateEvent args) => _movement.RefreshMovementSpeedModifiers(ent);
    private void OnIntoxicatedState(Entity<XenomorphIntoxicatedComponent> ent, ref AfterAutoHandleStateEvent args) => _movement.RefreshMovementSpeedModifiers(ent);
    private void OnIntoxicatedShutdown(Entity<XenomorphIntoxicatedComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Stacks = 0;
        if (!TerminatingOrDeleted(ent)) _movement.RefreshMovementSpeedModifiers(ent);
    }
}
