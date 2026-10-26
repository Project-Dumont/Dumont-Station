using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class XenomorphPraetorianComponent : Component
{
    [DataField, AutoNetworkedField] public float Shield;
    [DataField, AutoNetworkedField] public bool Preparing;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphAcidStacksComponent : Component
{
    [DataField, AutoNetworkedField] public int Current;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan LastIncrement;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan LastDecrement;
}

public sealed partial class XenomorphPraetorianSpitActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphAcidBallActionEvent : WorldTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphAcidBallDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public NetCoordinates Coordinates;
}

public sealed partial class SharedXenomorphPraetorianSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphPraetorianComponent, UpdateCanMoveEvent>(OnMove);
        SubscribeLocalEvent<XenomorphPraetorianComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphPraetorianComponent, ComponentShutdown>(OnShutdown);
    }
    private void OnMove(Entity<XenomorphPraetorianComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.Preparing && ent.Comp.Running) args.Cancel();
    }
    private void OnState(Entity<XenomorphPraetorianComponent> ent, ref AfterAutoHandleStateEvent args) => _blocker.UpdateCanMove(ent);
    private void OnShutdown(Entity<XenomorphPraetorianComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Preparing = false;
        if (!TerminatingOrDeleted(ent)) _blocker.UpdateCanMove(ent);
    }
}
