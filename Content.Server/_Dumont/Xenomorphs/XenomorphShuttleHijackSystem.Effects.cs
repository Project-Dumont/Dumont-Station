using System.Numerics;
using Content.Shared.Camera;
using Content.Server._CMU14.Xenomorphs;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphShuttleHijackSystem
{
    [Dependency] private SharedCameraRecoilSystem _recoil = default!;
    private readonly List<(TimeSpan At, EntityCoordinates Position, EntityUid Warning)> _pendingExplosions = new();

    private void DetonateWarnedExplosions()
    {
        for (var i = _pendingExplosions.Count - 1; i >= 0; i--)
        {
            var pending = _pendingExplosions[i];
            if (_timing.CurTime < pending.At) continue;
            _pendingExplosions.RemoveAt(i);
            if (Exists(pending.Warning)) QueueDel(pending.Warning);
            if (!Exists(pending.Position.EntityId)) continue;
            // A real blast, rather than the previous 20-intensity fire puff.
            _explosions.QueueExplosion(_transform.ToMapCoordinates(pending.Position), "Default", 300, 5, 20,
                cause: null, maxTileBreak: 3, canCreateVacuum: false);
            Spawn("CMUExplosionEffectGrenade", pending.Position);
            Spawn("CMUExplosionEffectShockWave", pending.Position);
            SpawnInvasionFire(pending.Position);
            PlayInvasionBlast(pending.Position, impact: false);
        }
    }

    private void SpawnInvasionFire(EntityCoordinates center)
    {
        if (!TryComp<MapGridComponent>(_centcomm, out var grid)) return;
        var origin = _map.TileIndicesFor(_centcomm.Value, grid, center);
        // CMU's pipe fires spread in a two-tile diamond, stopping at walls and space.
        for (var x = -2; x <= 2; x++)
        for (var y = -2; y <= 2; y++)
        {
            if (Math.Abs(x) + Math.Abs(y) > 2) continue;
            var tile = origin + new Vector2i(x, y);
            var coordinates = _map.GridTileToLocal(_centcomm.Value, grid, tile);
            if (_turf.IsSpace(_map.GetTileRef(_centcomm.Value, grid, coordinates))
                || !_interaction.InRangeUnobstructed(_transform.ToMapCoordinates(center), _transform.ToMapCoordinates(coordinates), 3f)) continue;
            var burning = false;
            foreach (var anchored in _map.GetAnchoredEntities(_centcomm.Value, grid, tile))
                if (HasComp<CMUTileFireComponent>(anchored)) { burning = true; break; }
            if (!burning) Spawn("CMUHijackPipeFire", coordinates);
        }
    }

    private void PlayLandingImpact(EntityUid shuttle)
    {
        var bounds = Comp<MapGridComponent>(shuttle).LocalAABB;
        var center = _transform.ToCoordinates(_centcomm!.Value, _transform.GetMapCoordinates(shuttle));
        center = new EntityCoordinates(center.EntityId, center.Position + bounds.Center);
        Spawn("CMUExplosionEffectShockWave", center);
        // Fireballs along the hull make the landing visible without detonating the passengers.
        for (var side = 0; side < 4; side++)
        for (var part = -1; part <= 1; part += 2)
        {
            var offset = side switch
            {
                0 => new Vector2(-bounds.Width / 2, part * bounds.Height / 4),
                1 => new Vector2(bounds.Width / 2, part * bounds.Height / 4),
                2 => new Vector2(part * bounds.Width / 4, -bounds.Height / 2),
                _ => new Vector2(part * bounds.Width / 4, bounds.Height / 2),
            };
            var position = new EntityCoordinates(center.EntityId, center.Position + offset);
            Spawn("CMUExplosionEffectGrenade", position);
        }
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/metal_slam5.ogg", AudioParams.Default.WithVolume(8)), center);
        PlayInvasionBlast(center, impact: true);
    }

    private void PlayInvasionBlast(EntityCoordinates position, bool impact)
    {
        _audio.PlayPvs(new SoundCollectionSpecifier(impact ? "DumontXenomorphBigExplosion" : "DumontXenomorphExplosion",
            AudioParams.Default.WithVolume(impact ? 6 : 2).WithMaxDistance(60)), position);
        var far = Filter.Empty();
        var epicenter = _transform.ToMapCoordinates(position);
        var players = EntityQueryEnumerator<ActorComponent, TransformComponent>();
        while (players.MoveNext(out var uid, out var actor, out var xform))
        {
            if (xform.MapUid != _centcommMap) continue;
            var delta = _transform.GetWorldPosition(uid) - epicenter.Position;
            var distance = delta.Length();
            if (distance > 35) far.AddPlayer(actor.PlayerSession);
            var direction = distance < 0.01f ? Vector2.UnitX : delta / distance;
            var strength = (impact ? 1f : 0.65f) * Math.Clamp(1 - distance / 100, 0.25f, 1);
            _recoil.KickCamera(uid, direction * strength);
        }
        _audio.PlayGlobal(new SoundCollectionSpecifier("ExplosionFar", AudioParams.Default.WithVolume(4)), far, true);
    }
}
