using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class XenomorphHideComponent : Component
{
    [DataField, AutoNetworkedField] public bool Hiding;
}

public sealed partial class XenomorphHideSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private MobStateSystem _mobs = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHideComponent, XenomorphHideActionEvent>(OnHide);
        SubscribeLocalEvent<XenomorphHideComponent, PreventCollideEvent>(OnCollide);
        SubscribeLocalEvent<XenomorphHideComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnHide(Entity<XenomorphHideComponent> ent, ref XenomorphHideActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent)) return;
        ent.Comp.Hiding = !ent.Comp.Hiding;
        Dirty(ent);
        _physics.RegenerateContacts(ent.Owner);
        _actions.SetToggled(args.Action.Owner, ent.Comp.Hiding);
        args.Handled = true;
    }

    private void OnCollide(Entity<XenomorphHideComponent> ent, ref PreventCollideEvent args)
    {
        if (ent.Comp.Hiding && (args.OtherFixture.CollisionLayer & (int)CollisionGroup.TableLayer) != 0
            && (args.OtherFixture.CollisionLayer & (int)CollisionGroup.Impassable) == 0) args.Cancelled = true;
    }

    private void OnState(Entity<XenomorphHideComponent> ent, ref AfterAutoHandleStateEvent args) => _physics.RegenerateContacts(ent.Owner);
}
