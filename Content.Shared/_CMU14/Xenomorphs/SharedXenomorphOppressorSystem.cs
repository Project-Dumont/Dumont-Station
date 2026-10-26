using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphOppressorSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphOppressorComponent, UpdateCanMoveEvent>(OnMove);
        SubscribeLocalEvent<XenomorphOppressorComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphOppressorComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMove(Entity<XenomorphOppressorComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.Preparing && ent.Comp.RootedPreparing) args.Cancel();
    }

    private void OnState(Entity<XenomorphOppressorComponent> ent, ref AfterAutoHandleStateEvent args) => _blocker.UpdateCanMove(ent);
    private void OnShutdown(Entity<XenomorphOppressorComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Preparing = false;
        if (!TerminatingOrDeleted(ent)) _blocker.UpdateCanMove(ent);
    }
}
