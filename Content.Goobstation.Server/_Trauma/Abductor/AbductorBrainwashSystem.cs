// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Mindcontrol;
using Content.Goobstation.Shared.Mindcontrol;
using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared._Trauma.Mindcontrol;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server._Trauma.Abductor;

public sealed partial class AbductorBrainwashSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MindcontrolSystem _mindcontrol = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbductorGizmoComponent, BrainwashDoAfterEvent>(OnBrainwashDoAfterEvent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<TimedMindControlComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.ExpiresAt) continue;
            RemCompDeferred(uid, comp);
            RemCompDeferred<MindcontrolledComponent>(uid);
        }
    }

    private void OnBrainwashDoAfterEvent(Entity<AbductorGizmoComponent> ent, ref BrainwashDoAfterEvent args)
    {
        if (args.Cancelled || args.Target is not {} target)
            return;

        if (HasComp<MindShieldComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("abductors-gizmo-brainwash-mindshield"), target, args.User);
            return;
        }

        var comp = EnsureComp<MindcontrolledComponent>(target);
        comp.Master = args.User;
        comp.MindcontrolIcon = "AbductorMindControl";
        _mindcontrol.Start(target, comp);

        var timed = EnsureComp<TimedMindControlComponent>(target);
        timed.ExpiresAt = _timing.CurTime + TimeSpan.FromMinutes(15);
    }
}
