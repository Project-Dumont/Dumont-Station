using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server.DoAfter;
using Content.Server.Atmos.EntitySystems;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphTrapAcidComponent : Component
{
    public HashSet<EntityUid> Hit = new();
    public HashSet<EntityUid> Chain = new();
}

public sealed partial class XenomorphAcidTrapSystem : EntitySystem
{
    [Dependency] private XenomorphAciderSystem _acider = default!;
    [Dependency] private XenomorphSpitterSystem _spitter = default!;
    [Dependency] private FlammableSystem _fire = default!;
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphResinHoleComponent, InteractHandEvent>(OnFill);
        SubscribeLocalEvent<XenomorphResinHoleComponent, XenomorphFillAcidHoleDoAfterEvent>(OnFilled);
    }

    private bool CanFill(EntityUid user, Entity<XenomorphResinHoleComponent> hole) => !hole.Comp.Filled
        && (HasComp<XenomorphBurrowComponent>(user) || HasComp<XenomorphAciderComponent>(user)) && _mobs.IsAlive(user)
        && _blocker.CanInteract(user, hole) && _interaction.InRangeUnobstructed(user, hole.Owner, 1.5f)
        && (TryComp<XenomorphAciderComponent>(user, out var acider) ? acider.Acid >= 75 : _plasma.HasPlasma(user, 100));

    private void OnFill(Entity<XenomorphResinHoleComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !CanFill(args.User, ent)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, 3,
            new XenomorphFillAcidHoleDoAfterEvent(), ent.Owner, ent.Owner)
        { NeedHand = false, BreakOnMove = true, BreakOnDamage = true });
    }

    private void OnFilled(Entity<XenomorphResinHoleComponent> ent, ref XenomorphFillAcidHoleDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !CanFill(args.User, ent)) return;
        if (TryComp<XenomorphAciderComponent>(args.User, out var acider))
        {
            if (!_acider.SpendAcid((args.User, acider), 75)) return;
        }
        else if (!_plasma.ChangePlasmaAmount(args.User, -100)) return;
        ent.Comp.Filled = ent.Comp.Acid = true;
        _appearance.SetData(ent, XenomorphCarrierVisuals.TrapContent, XenomorphTrapContent.Acid);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/acid_sizzle1.ogg"), ent);
        args.Handled = true;
    }

    public void Burst(EntityUid hole)
    {
        var coordinates = _transform.GetMoverCoordinates(hole);
        var chain = new HashSet<EntityUid>();
        for (var x = -1; x <= 1; x++)
        for (var y = -1; y <= 1; y++)
        {
            var tile = new EntityCoordinates(coordinates.EntityId, coordinates.X + x, coordinates.Y + y);
            if (!_interaction.InRangeUnobstructed(hole, tile, 2)) continue;
            _atmos.HotspotExtinguish(tile.EntityId, new Vector2i((int)MathF.Floor(tile.X), (int)MathF.Floor(tile.Y)));
            var pool = Spawn("DumontXenomorphTrapAcid", tile);
            Comp<XenomorphTrapAcidComponent>(pool).Chain = chain;
        }
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/acid_sizzle3.ogg"), hole);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphTrapAcidComponent>();
        while (query.MoveNext(out var uid, out var acid))
        {
            foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(_transform.GetMoverCoordinates(uid), 0.5f))
            {
                _fire.Extinguish(target);
                if (acid.Hit.Contains(target) || HasComp<XenomorphComponent>(target)
                    || HasComp<Content.Server._White.Xenomorphs.FaceHugger.FaceHuggerComponent>(target)
                    || _mobs.IsDead(target) || !_interaction.InRangeUnobstructed(uid, target.Owner, 0.75f)) continue;
                acid.Hit.Add(target);
                var damage = new DamageSpecifier();
                damage.DamageDict["Caustic"] = acid.Chain.Add(target) ? 63.9 : 21.3;
                _damage.TryChangeDamage(target, damage);
                _spitter.ShowAcidSplash(target);
                _spitter.EnhanceAcid(target);
                if (!TryComp<Content.Shared.Standing.StandingStateComponent>(target, out var standing) || standing.Standing)
                    _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(4));
            }
        }
    }
}
