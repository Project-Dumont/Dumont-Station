// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Lock;
using Content.Shared.Trigger.Components.Effects;

namespace Content.Shared.Trigger.Systems;

public sealed partial class LockOnTriggerSystem : EntitySystem
{
    [Dependency] private LockSystem _lock = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LockOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<LockOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;
        var target = ent.Comp.TargetUser ? args.User : ent.Owner;
        if (target == null || !TryComp<LockComponent>(target, out var comp))
            return;
        switch (ent.Comp.LockMode)
        {
            case LockAction.Lock: _lock.Lock(target.Value, args.User, comp); break;
            case LockAction.Unlock: _lock.Unlock(target.Value, args.User, comp); break;
            case LockAction.Toggle:
                if (comp.Locked)
                    _lock.Unlock(target.Value, args.User, comp);
                else
                    _lock.Lock(target.Value, args.User, comp);
                break;
        }
        args.Handled = true;
    }
}
