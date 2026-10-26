using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphOppressorSystem
{
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;
    private readonly HashSet<EntityUid> _areaTargets = new();

    private void OnAbduct(Entity<XenomorphOppressorComponent> ent, ref XenomorphAbductActionEvent args)
    {
        if (args.Handled || ent.Comp.Preparing || !Ready(ent) || !_plasma.HasPlasma(ent, 180)) return;
        if (!BuildTelegraphs(ent, args.Target, false)) return;
        args.Handled = Prepare(ent, args.Action.Owner, false);
    }

    private void OnLash(Entity<XenomorphOppressorComponent> ent, ref XenomorphTailLashActionEvent args)
    {
        if (args.Handled || ent.Comp.Preparing || !Ready(ent) || !_plasma.HasPlasma(ent, 80)) return;
        if (!BuildTelegraphs(ent, args.Target, true)) return;
        args.Handled = Prepare(ent, args.Action.Owner, true);
    }

    private bool BuildTelegraphs(Entity<XenomorphOppressorComponent> ent, EntityCoordinates target, bool lash)
    {
        if (Transform(ent).GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid)) return false;
        var mapOrigin = _transform.GetMapCoordinates(ent);
        var mapTarget = _transform.ToMapCoordinates(target);
        if (mapOrigin.MapId != mapTarget.MapId) return false;
        var from = _transform.ToCoordinates(gridUid, mapOrigin);
        var to = _transform.ToCoordinates(gridUid, mapTarget);
        var delta = to.Position - from.Position;
        if (!float.IsFinite(delta.LengthSquared()) || delta.LengthSquared() < 0.0001f) return false;
        var direction = Vector2.Normalize(delta);
        ClearTelegraphs(ent);
        if (lash)
        {
            var area = Box2.CenteredAround(from.Position, new Vector2(3, 2)).Translated(new Vector2(0, 1.5f));
            var rotated = new Box2Rotated(area, direction.ToAngle() - Angle.FromDegrees(90), from.Position);
            var worldArea = new Box2Rotated(area.Translated(mapOrigin.Position - from.Position),
                rotated.Rotation + _transform.GetWorldRotation(gridUid), mapOrigin.Position);
            ent.Comp.LashArea = worldArea;
            ent.Comp.LashGrid = gridUid;
            var bounds = rotated.CalcBoundingBox();
            foreach (var tile in _map.GetTilesIntersecting(gridUid, grid, worldArea))
            {
                var point = _turf.GetTileCenter(tile);
                if (!_interaction.InRangeUnobstructed(ent.Owner, point, 6, collisionMask: CollisionGroup.Impassable)) continue;
                var effect = bounds.Encloses(Box2.CenteredAround(point.Position, Vector2.One)) ? "DumontXenomorphLashTelegraph" : "DumontXenomorphLashEdgeTelegraph";
                ent.Comp.Telegraphs.Add(Spawn(effect, point));
            }
        }
        else
        {
            var seen = new HashSet<Vector2>();
            for (var i = 1; i <= MathF.Ceiling(Math.Min(delta.Length(), 6)); i++)
            {
                var point = from.Position + direction * i;
                point = new Vector2(MathF.Floor(point.X) + 0.5f, MathF.Floor(point.Y) + 0.5f);
                if (!seen.Add(point)) continue;
                var coordinates = new EntityCoordinates(gridUid, point);
                if (!_interaction.InRangeUnobstructed(ent.Owner, coordinates, 7)) break;
                ent.Comp.Telegraphs.Add(Spawn("DumontXenomorphAbductTelegraph", coordinates));
            }
        }
        if (ent.Comp.Telegraphs.Count > 0) return true;
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-oppressor-no-room"), ent, ent);
        return false;
    }

    private bool Prepare(Entity<XenomorphOppressorComponent> ent, EntityUid action, bool lash)
    {
        if (!_doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(lash ? 0.2 : 0.8),
            new XenomorphOppressorDoAfterEvent { Lash = lash }, ent) { NeedHand = false, BreakOnMove = true }))
        {
            ClearTelegraphs(ent);
            return false;
        }
        ent.Comp.Preparing = true;
        ent.Comp.RootedPreparing = !lash;
        ent.Comp.PreparingAction = action;
        Dirty(ent);
        _blocker.UpdateCanMove(ent);
        _actions.SetToggled(action, true);
        if (!lash) _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphRoar"), ent);
        return true;
    }

    private void ClearTelegraphs(Entity<XenomorphOppressorComponent> ent)
    {
        foreach (var uid in ent.Comp.Telegraphs) if (Exists(uid)) QueueDel(uid);
        ent.Comp.Telegraphs.Clear();
    }

    private void EndPreparation(Entity<XenomorphOppressorComponent> ent)
    {
        ClearTelegraphs(ent);
        ent.Comp.Preparing = false;
        ent.Comp.RootedPreparing = false;
        ent.Comp.LashArea = null;
        ent.Comp.LashGrid = null;
        if (ent.Comp.PreparingAction is { } action && Exists(action)) _actions.SetToggled(action, false);
        ent.Comp.PreparingAction = null;
        if (TerminatingOrDeleted(ent) || !ent.Comp.Running) return;
        Dirty(ent);
        _blocker.UpdateCanMove(ent);
    }

    private void OnRemoved(Entity<XenomorphOppressorComponent> ent, ref ComponentRemove args) => EndPreparation(ent);

    private void Cooldown(EntityUid action, double seconds)
    {
        if (!Exists(action)) return;
        _actions.SetCooldown(action, TimeSpan.FromSeconds(seconds));
    }

    private void OnPrepared(Entity<XenomorphOppressorComponent> ent, ref XenomorphOppressorDoAfterEvent args)
    {
        if (args.Handled) return;
        var action = ent.Comp.PreparingAction;
        var points = ent.Comp.Telegraphs.Where(Exists).Select(uid => Transform(uid).Coordinates).ToArray();
        var area = ent.Comp.LashArea;
        var grid = ent.Comp.LashGrid;
        EndPreparation(ent);
        if (action == null) return;
        if (!args.Lash) Cooldown(action.Value, 15);
        if (args.Cancelled || !Ready(ent) || !_plasma.ChangePlasmaAmount(ent, args.Lash ? -80 : -180))
        {
            if (!args.Lash) _popup.PopupEntity(Loc.GetString("dumont-xenomorph-abduct-cancel"), ent, ent);
            return;
        }
        args.Handled = true;
        if (args.Lash)
        {
            Cooldown(action.Value, 13);
            if (area == null || grid == null || !Exists(grid)) return;
            EnsureComp<XenomorphSweepingComponent>(ent);
            _areaTargets.Clear();
            _lookup.GetEntitiesIntersecting(grid.Value, area.Value, _areaTargets, LookupFlags.Dynamic | LookupFlags.Static);
            foreach (var target in _areaTargets)
            {
                if (!Victim(target) || !_interaction.InRangeUnobstructed(ent.Owner, target, 6, collisionMask: CollisionGroup.Impassable)) continue;
                _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(1));
                Slow(target, 2.5);
                Throw(ent, target, 3);
            }
        }
        else
        {
            var targets = new HashSet<EntityUid>();
            foreach (var point in points)
            {
                _nearby.Clear();
                _lookup.GetEntitiesInRange(point, 0.4f, _nearby);
                foreach (var mob in _nearby)
                    if (Victim(mob) && _mobs.IsAlive(mob) && !HasComp<StunnedComponent>(mob)) targets.Add(mob);
            }
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_footstep_charge1.ogg"), ent);
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-abduct-result", ("count", targets.Count)), ent, ent, PopupType.Medium);
            var position = _transform.GetWorldPosition(ent);
            foreach (var target in targets.OrderBy(uid => Vector2.DistanceSquared(position, _transform.GetWorldPosition(uid))).ThenBy(uid => uid.Id).Take(10))
            {
                if (!Hook(ent, target, false)) continue;
                if (targets.Count == 1) Slow(target, 2.5);
                else if (targets.Count == 2)
                {
                    Root(target, 2.5);
                    _rage.Daze(target);
                    _stutter.DoStutter(target, TimeSpan.FromSeconds(4), true);
                }
                _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(targets.Count > 2 ? 2.6 : 0.4));
            }
        }
    }
}
