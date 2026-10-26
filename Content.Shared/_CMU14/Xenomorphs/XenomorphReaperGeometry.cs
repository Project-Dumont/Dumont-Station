using Content.Shared._Dumont.Xenomorphs;
using System.Numerics;

namespace Content.Shared._CMU14.Xenomorphs;

public static class XenomorphReaperGeometry
{
    public static Vector2 Snap(Vector2 point) => new(MathF.Floor(point.X) + 0.5f, MathF.Floor(point.Y) + 0.5f);

    public static List<Vector2> GasPath(Vector2 start, Vector2 target, float range)
    {
        start = Snap(start);
        var delta = Snap(target) - start;
        var steps = (int)MathF.Round(Math.Min(range, delta.Length()));
        var direction = delta.LengthSquared() > 0.001f ? Vector2.Normalize(delta) : Vector2.Zero;
        var path = new List<Vector2>();
        for (var i = 0; i <= steps; i++)
        {
            var point = Snap(start + direction * i);
            if (path.Count == 0 || path[^1] != point) path.Add(point);
        }
        return path;
    }

    public static IEnumerable<Vector2> GasTiles(IReadOnlyList<Vector2> path, int step)
    {
        var center = path[step];
        yield return center;
        var progress = path.Count > 1 ? step / (float)(path.Count - 1) : 0;
        var direction = path[Math.Min(path.Count - 1, step + 1)] - path[Math.Max(0, step - 1)];
        var side = MathF.Abs(direction.Y) >= MathF.Abs(direction.X)
            ? new Vector2(direction.Y >= 0 ? -1 : 1, 0) : new Vector2(0, direction.X >= 0 ? 1 : -1);
        if (progress >= 1f / 3) yield return center + side;
        if (progress >= 2f / 3) yield return center - side;
    }
}
