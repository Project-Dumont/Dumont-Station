using System.Numerics;

namespace Content.Shared.Weapons.Reflect;

public sealed partial class ReflectSystem
{
    /// <summary>
    /// Gets the rotation that turns a projectile's velocity so it leaves in the reflector's override direction.
    /// </summary>
    private Angle GetOverrideRotation(EntityUid reflector, Vector2 velocity, Angle overrideAngle)
    {
        var target = _transform.GetWorldRotation(reflector) + overrideAngle;
        return target - velocity.ToAngle();
    }

    /// <summary>
    /// Gets the direction a hitscan leaves in when reflected by a reflector with an override angle.
    /// </summary>
    private Vector2 GetOverrideDirection(EntityUid reflector, Angle overrideAngle)
    {
        var target = _transform.GetWorldRotation(reflector) + overrideAngle;
        return new Vector2((float) Math.Cos(target), (float) Math.Sin(target));
    }
}
