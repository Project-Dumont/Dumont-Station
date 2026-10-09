// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Shared.Damage;

using Content.Shared.Weapons.Melee.Events;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Damage.Systems;
using Content.Trauma.Shared.Heretic.Components.Side;
using Content.Trauma.Shared.Heretic.Systems.Abilities;
using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Blade;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems.Side;

public sealed partial class SharedUnfathomableCurioSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // Dumont start
        SubscribeLocalEvent<UnfathomableCurioShieldComponent, BeforeHarmfulActionEvent>(OnBeforeHarmfulAction, after: new[] { typeof(SharedHereticAbilitySystem), typeof(RiposteeSystem) });
        // Dumont end
    }

    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<UnfathomableCurioShieldComponent>();
        while (query.MoveNext(out var uid, out var shield))
        {
            if (shield.Active)
                continue;

            if (now < shield.ActivateTime)
                continue;

            shield.Active = true;
            Dirty(uid, shield);
            _audio.PlayPvs(shield.RechargeSound, uid);
        }
    }

    private void OnBeforeHarmfulAction(Entity<UnfathomableCurioShieldComponent> ent, ref BeforeHarmfulActionEvent args)
    {
        if (!ent.Comp.Active || args.Cancelled || args.Type != HarmfulActionType.Harm)
            return;

        args.Cancel();
        ResetShield(ent, true, args.User);
    }

    [SubscribeLocalEvent]
    private void OnTakeDamage(Entity<UnfathomableCurioShieldComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.Cancelled || args.Damage.GetTotal() < 5)
            return;

        if (!ent.Comp.Active)
        {
            ent.Comp.ActivateTime = _timing.CurTime + ent.Comp.ActivateDelay;
            Dirty(ent);
            return;
        }

        args.Cancelled = true;
        ResetShield(ent, true, args.Origin);
    }

    [SubscribeLocalEvent]
    private void OnInit(Entity<UnfathomableCurioShieldComponent> ent, ref MapInitEvent args)
    {
        ResetShield(ent, false, null);
    }

    private void ResetShield(Entity<UnfathomableCurioShieldComponent> ent, bool playSound, EntityUid? origin)
    {
        ent.Comp.Active = false;
        ent.Comp.ActivateTime = _timing.CurTime + ent.Comp.ActivateDelay;
        Dirty(ent);

        if (!playSound)
            return;

        _audio.PlayPredicted(ent.Comp.BlockSound, ent, origin);
    }
}
