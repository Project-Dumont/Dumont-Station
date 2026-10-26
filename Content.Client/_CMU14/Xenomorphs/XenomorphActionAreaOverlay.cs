using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using System.Numerics;
using Content.Client.UserInterface.Systems.Actions;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.GameObjects;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphActionAreaOverlay : Overlay
{
    [Dependency] private IEntityManager _entities = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IUserInterfaceManager _ui = default!;
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IInputManager _input = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public XenomorphActionAreaOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } user
            || _ui.GetUIController<ActionUIController>().SelectingTargetFor is not { } action
            || !_entities.TryGetComponent<XenomorphActionAreaComponent>(action, out var area))
            return;

        var transform = _entities.System<SharedTransformSystem>();
        var origin = transform.GetMapCoordinates(user);
        var target = _eye.ScreenToMap(_input.MouseScreenPosition);
        if (origin.MapId != args.MapId || target.MapId != origin.MapId)
            return;

        var range = area.Range;
        if (_entities.TryGetComponent<XenomorphDespoilerComponent>(user, out var despoiler)
            && _entities.System<SharedActionsSystem>().GetEvent(action) is XenomorphCausticEmbraceActionEvent)
            range = despoiler.Empowered ? 5 : 3;

        if (area.TargetCircle)
        {
            var delta = target.Position - origin.Position;
            var center = delta.LengthSquared() > area.TargetRange * area.TargetRange
                ? origin.Position + Vector2.Normalize(delta) * area.TargetRange : target.Position;
            origin = new MapCoordinates(center, origin.MapId);
        }
        var direction = target.Position - origin.Position;
        var facing = MathF.Atan2(direction.Y, direction.X);
        var arc = MathHelper.DegreesToRadians(area.Angle);
        var start = facing - arc / 2;
        var color = Color.FromHex("#9AD26B").WithAlpha(0.8f);
        var handle = args.WorldHandle;
        if ((area.TargetSquare || area.WideningSpray) && _entities.GetComponent<TransformComponent>(user).GridUid is { } reaperGrid)
        {
            var from = transform.ToCoordinates(reaperGrid, origin).Position;
            var to = transform.ToCoordinates(reaperGrid, target).Position;
            var interaction = _entities.System<Content.Shared.Interaction.SharedInteractionSystem>();
            var tiles = new List<Vector2>();
            if (area.TargetSquare)
            {
                var center = XenomorphReaperGeometry.Snap(to);
                if (!interaction.InRangeUnobstructed(user, new EntityCoordinates(reaperGrid, center), range)) return;
                for (var x = -1; x <= 1; x++) for (var y = -1; y <= 1; y++)
                {
                    var tile = center + new Vector2(x, y);
                    if (interaction.InRangeUnobstructed(transform.ToMapCoordinates(new EntityCoordinates(reaperGrid, center)), transform.ToMapCoordinates(new EntityCoordinates(reaperGrid, tile)), 2))
                        tiles.Add(tile);
                }
            }
            else
            {
                var path = XenomorphReaperGeometry.GasPath(from, to, range);
                for (var step = 0; step < path.Count; step++)
                foreach (var tile in XenomorphReaperGeometry.GasTiles(path, step))
                    if (interaction.InRangeUnobstructed(transform.ToMapCoordinates(new EntityCoordinates(reaperGrid, path[0])), transform.ToMapCoordinates(new EntityCoordinates(reaperGrid, tile)), range + 2))
                        tiles.Add(tile);
            }
            foreach (var tile in tiles)
            {
                var a = transform.ToMapCoordinates(new EntityCoordinates(reaperGrid, tile + new Vector2(-0.5f, -0.5f))).Position;
                var b = transform.ToMapCoordinates(new EntityCoordinates(reaperGrid, tile + new Vector2(0.5f, -0.5f))).Position;
                var c = transform.ToMapCoordinates(new EntityCoordinates(reaperGrid, tile + new Vector2(0.5f, 0.5f))).Position;
                var d = transform.ToMapCoordinates(new EntityCoordinates(reaperGrid, tile + new Vector2(-0.5f, 0.5f))).Position;
                handle.DrawLine(a, b, color);
                handle.DrawLine(b, c, color);
                handle.DrawLine(c, d, color);
                handle.DrawLine(d, a, color);
            }
            return;
        }
        if (area.SprayLine && _entities.GetComponent<TransformComponent>(user).GridUid is { } sprayGrid)
        {
            var from = transform.ToCoordinates(sprayGrid, origin).Position;
            var to = transform.ToCoordinates(sprayGrid, target).Position;
            from = new Vector2(MathF.Floor(from.X) + 0.5f, MathF.Floor(from.Y) + 0.5f);
            to = new Vector2(MathF.Floor(to.X) + 0.5f, MathF.Floor(to.Y) + 0.5f);
            var delta = to - from;
            var length = delta.Length();
            if (length < 0.1f)
                return;
            var previousTile = from;
            var interaction = _entities.System<Content.Shared.Interaction.SharedInteractionSystem>();
            for (var i = 1; i <= MathF.Ceiling(Math.Min(length, range)); i++)
            {
                var tile = from + delta / length * i;
                tile = new Vector2(MathF.Floor(tile.X) + 0.5f, MathF.Floor(tile.Y) + 0.5f);
                if (tile == previousTile)
                    continue;
                previousTile = tile;
                if (!interaction.InRangeUnobstructed(user, new EntityCoordinates(sprayGrid, tile), range: range + 1))
                    break;
                var a = transform.ToMapCoordinates(new EntityCoordinates(sprayGrid, tile + new Vector2(-0.5f, -0.5f))).Position;
                var b = transform.ToMapCoordinates(new EntityCoordinates(sprayGrid, tile + new Vector2(0.5f, -0.5f))).Position;
                var c = transform.ToMapCoordinates(new EntityCoordinates(sprayGrid, tile + new Vector2(0.5f, 0.5f))).Position;
                var d = transform.ToMapCoordinates(new EntityCoordinates(sprayGrid, tile + new Vector2(-0.5f, 0.5f))).Position;
                handle.DrawLine(a, b, color);
                handle.DrawLine(b, c, color);
                handle.DrawLine(c, d, color);
                handle.DrawLine(d, a, color);
            }
            return;
        }
        if (area.GridTiles && _entities.GetComponent<TransformComponent>(user).GridUid is { } grid)
        {
            var from = transform.ToCoordinates(grid, origin);
            var to = transform.ToCoordinates(grid, target);
            var delta = to.Position - from.Position;
            if (delta.LengthSquared() < 0.0001f)
                return;
            var forward = Math.Abs(delta.X) >= Math.Abs(delta.Y) ? new Vector2(Math.Sign(delta.X), 0) : new Vector2(0, Math.Sign(delta.Y));
            var side = new Vector2(-forward.Y, forward.X);
            var center = new Vector2(MathF.Floor(from.X) + 0.5f, MathF.Floor(from.Y) + 0.5f);
            for (var distance = 1; distance <= (int) range; distance++)
            for (var width = -(int) area.Width / 2; width <= (int) area.Width / 2; width++)
            {
                var tile = center + forward * distance + side * width;
                var a = transform.ToMapCoordinates(new EntityCoordinates(grid, tile + new Vector2(-0.5f, -0.5f))).Position;
                var b = transform.ToMapCoordinates(new EntityCoordinates(grid, tile + new Vector2(0.5f, -0.5f))).Position;
                var c = transform.ToMapCoordinates(new EntityCoordinates(grid, tile + new Vector2(0.5f, 0.5f))).Position;
                var d = transform.ToMapCoordinates(new EntityCoordinates(grid, tile + new Vector2(-0.5f, 0.5f))).Position;
                handle.DrawLine(a, b, color);
                handle.DrawLine(b, c, color);
                handle.DrawLine(c, d, color);
                handle.DrawLine(d, a, color);
            }
            return;
        }
        if (area.Width > 0)
        {
            if (direction.LengthSquared() < 0.0001f)
                return;
            var forward = Vector2.Normalize(direction) * range;
            var side = Vector2.Normalize(new Vector2(-direction.Y, direction.X)) * (area.Width / 2);
            var offset = Vector2.Normalize(direction) * area.Offset;
            var left = origin.Position + side + offset;
            var right = origin.Position - side + offset;
            handle.DrawLine(left, left + forward, color);
            handle.DrawLine(right, right + forward, color);
            handle.DrawLine(left + forward, right + forward, color);
            handle.DrawLine(left, right, color);
            return;
        }
        var previous = origin.Position + new Vector2(MathF.Cos(start), MathF.Sin(start)) * range;
        if (area.Angle < 360)
            handle.DrawLine(origin.Position, previous, color);
        for (var i = 1; i <= 64; i++)
        {
            var angle = start + arc * i / 64;
            var point = origin.Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * range;
            handle.DrawLine(previous, point, color);
            previous = point;
        }
        if (area.Angle < 360)
            handle.DrawLine(origin.Position, previous, color);
    }
}
