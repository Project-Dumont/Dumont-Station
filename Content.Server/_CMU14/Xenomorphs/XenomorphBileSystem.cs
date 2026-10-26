using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server.Atmos.EntitySystems;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Atmos.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphBileSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private FlammableSystem _fire = default!;
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    private readonly HashSet<Entity<FlammableComponent>> _nearby = new();
    private readonly HashSet<EntityUid> _targets = new();
    private readonly List<EntityCoordinates> _tiles = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHeatshieldComponent, XenomorphVomitBileActionEvent>(OnBile);
    }

    private bool CanTarget(EntityUid user, EntityUid target)
        => user != target && Exists(target) && TryComp<FlammableComponent>(target, out var fire)
           && (HasComp<XenomorphComponent>(target) ? fire.OnFire && fire.CanExtinguish : HasComp<MobStateComponent>(target) && !_mobs.IsDead(target))
           && _interaction.InRangeUnobstructed(user, target, range: 5.4f);

    private void OnBile(Entity<XenomorphHeatshieldComponent> ent, ref XenomorphVomitBileActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent) || !_plasma.HasPlasma(ent, 40))
            return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        if (origin.MapId != target.MapId || (target.Position - origin.Position).LengthSquared() > 5.4f * 5.4f)
            return;
        if (args.Entity is { } ally && HasComp<XenomorphComponent>(ally) && CanTarget(ent, ally))
        {
            _fire.Extinguish(ally);
            args.Handled = true;
            return;
        }
        if (Transform(ent).GridUid is not { } grid)
            return;
        var local = _transform.ToCoordinates(grid, target);
        var tile = new Vector2i((int) MathF.Floor(local.X), (int) MathF.Floor(local.Y));
        var selectedEnemy = args.Entity is { } selected && !HasComp<XenomorphComponent>(selected) && CanTarget(ent, selected);
        if (!selectedEnemy && _atmos.IsHotspotActive(grid, tile))
        {
            _atmos.HotspotExtinguish(grid, tile);
            args.Handled = true;
            return;
        }
        _targets.Clear();
        _tiles.Clear();
        if (selectedEnemy)
            _targets.Add(args.Entity!.Value);
        var from = _transform.ToCoordinates(grid, origin);
        var delta = local.Position - from.Position;
        if (delta.LengthSquared() > 0.0001f)
        {
            var forward = Math.Abs(delta.X) >= Math.Abs(delta.Y) ? new Vector2(Math.Sign(delta.X), 0) : new Vector2(0, Math.Sign(delta.Y));
            var side = new Vector2(-forward.Y, forward.X);
            var center = new Vector2(MathF.Floor(from.X) + 0.5f, MathF.Floor(from.Y) + 0.5f);
            for (var distance = 1; distance <= 3; distance++)
            for (var width = -1; width <= 1; width++)
            {
                var coordinates = new EntityCoordinates(grid, center + forward * distance + side * width);
                if (!_interaction.InRangeUnobstructed(ent.Owner, coordinates, range: 5.4f))
                    continue;
                _tiles.Add(coordinates);
                _nearby.Clear();
                _lookup.GetEntitiesInRange(_transform.ToMapCoordinates(coordinates), 0.65f, _nearby);
                foreach (var candidate in _nearby)
                    if (CanTarget(ent, candidate))
                        _targets.Add(candidate);
            }
        }
        if (_targets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-bile-no-target"), ent, ent);
            return;
        }
        args.Handled = true;
        var burning = TryComp<FlammableComponent>(ent, out var fire) && fire.OnFire;
        if (burning)
        {
            _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphFireSpew", AudioParams.Default.WithVolume(-6).WithVariation(0.05f)), ent);
            foreach (var coordinates in _tiles)
                Spawn("DumontXenomorphFireSpew", coordinates);
        }
        foreach (var victim in _targets)
        {
            if (HasComp<XenomorphComponent>(victim))
                _fire.Extinguish(victim);
            else
                _fire.AdjustFireStacks(victim, 4, ignite: burning);
        }
    }
}
