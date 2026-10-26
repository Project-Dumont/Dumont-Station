using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Server._White.Xenomorphs.FaceHugger;
using Content.Shared.Buckle.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphRageSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphRageComponent, MeleeHitEvent>(OnMelee);
        InitializeAbilities();
        SubscribeLocalEvent<XenomorphRageComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<XenomorphRageComponent, ComponentRemove>(OnRemove);
    }

    private void OnExamine(Entity<XenomorphRageComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<XenomorphComponent>(args.Examiner))
            args.PushMarkup(Loc.GetString("xenomorph-rage-examine", ("amount", ent.Comp.Rage)));
    }

    private bool Enemy(EntityUid uid) => HasComp<MobStateComponent>(uid) && !_mobs.IsDead(uid)
        && !HasComp<XenomorphComponent>(uid) && !HasComp<FaceHuggerComponent>(uid)
        && !_containers.IsEntityInContainer(uid)
        && !(TryComp<BuckleComponent>(uid, out var buckle) && buckle.BuckledTo is { } nest && HasComp<XenomorphNestComponent>(nest));

    public void ChangeRage(Entity<XenomorphRageComponent> ent, int amount)
    {
        if (ent.Comp.Locked || ent.Comp.CooldownUntil > _timing.CurTime) return;
        ent.Comp.Rage = Math.Clamp(ent.Comp.Rage + amount, 0, 5);
        if (ent.Comp.Rage == 5)
        {
            ent.Comp.Locked = true;
            ent.Comp.LockUntil = _timing.CurTime + TimeSpan.FromSeconds(10);
            GiveAura(ent, Color.Black, 3, 10);
            _popup.PopupEntity(Loc.GetString("xenomorph-rage-locked"), ent, ent);
        }
        Refresh(ent);
    }

    private void Refresh(Entity<XenomorphRageComponent> ent)
    {
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
        _armor.RefreshAlert(ent);
    }

    private void OnMelee(Entity<XenomorphRageComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.User != ent.Owner || _mobs.IsDead(ent) || !args.HitEntities.Any(Enemy)) return;
        ChangeRage(ent, 1);
        QueueHeal(ent, (0.05f * ent.Comp.Rage + 0.3f) * 65);
        ApprehendHit(ent, args);
        ent.Comp.LastHit = _timing.CurTime;
    }

    private void OnRemove(Entity<XenomorphRageComponent> ent, ref ComponentRemove args)
    {
        ent.Comp.Rage = 0;
        ent.Comp.ApprehendUntil = TimeSpan.Zero;
        ToggleApprehend(ent, false);
        ToggleEviscerate(ent, false);
        _armor.RefreshAlert(ent);
    }

    private void Heal(Entity<XenomorphRageComponent> ent)
    {
        var remaining = ent.Comp.PendingHeal;
        ent.Comp.PendingHeal = 0;
        if (_mobs.IsDead(ent) || !TryComp<DamageableComponent>(ent, out var damage)) return;
        var heal = new DamageSpecifier();
        foreach (var type in new[] { "Blunt", "Slash", "Piercing", "Heat", "Shock", "Cold", "Caustic" })
        {
            if (!damage.Damage.DamageDict.TryGetValue(type, out var amount) || amount <= 0 || remaining <= 0) continue;
            var restored = Math.Min(amount.Float(), remaining);
            heal.DamageDict[type] = FixedPoint2.New(-restored);
            remaining -= restored;
        }
        _damage.TryChangeDamage(ent, heal, ignoreResistances: true);
    }

    public override void Update(float frameTime)
    {
        UpdateAbilities();
        var query = EntityQueryEnumerator<XenomorphRageComponent>();
        while (query.MoveNext(out var uid, out var rage))
        {
            if (rage.PendingHeal > 0 && rage.HealAt <= _timing.CurTime) Heal((uid, rage));
            if (rage.Locked && rage.LockUntil <= _timing.CurTime)
            {
                rage.Locked = false;
                rage.Rage = 0;
                rage.CooldownUntil = _timing.CurTime + TimeSpan.FromSeconds(10);
                GiveAura((uid, rage), Color.White, 2, 10);
                Refresh((uid, rage));
                _popup.PopupEntity(Loc.GetString("xenomorph-rage-expired"), uid, uid);
            }
            else if (!rage.Locked && rage.Rage > 0 && rage.LastHit + TimeSpan.FromSeconds(5) <= _timing.CurTime)
            {
                ChangeRage((uid, rage), -1);
                rage.LastHit = _timing.CurTime;
            }
        }
    }
}
