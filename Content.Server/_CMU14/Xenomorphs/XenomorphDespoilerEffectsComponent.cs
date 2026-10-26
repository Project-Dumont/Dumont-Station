using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphDespoilerPoolComponent : Component
{
    public EntityUid? Caster;
    [DataField] public bool Spray;
    [DataField] public bool Empowered;
    public HashSet<EntityUid> Hit = new();
}
[RegisterComponent]
public sealed partial class XenomorphDespoilerProjectileComponent : Component
{
    public EntityUid Caster;
    public MapCoordinates Origin;
    public float Range;
}
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphCausticLeapComponent : Component
{
    public EntityUid? Victim;
    public bool Empowered;
    public bool Resolved;
    public MapCoordinates Destination;
    public Vector2 Direction;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan EndsAt;
}
