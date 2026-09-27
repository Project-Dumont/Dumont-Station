using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Dumont.Revolutionary;

[RegisterComponent, Access(typeof(RevForcesFollowerSystem))]
public sealed partial class RevForcesFollowerComponent : Component
{
    [DataField]
    public EntityUid? Target;

    [DataField]
    public TimeSpan CheckInterval = TimeSpan.FromSeconds(2);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextCheck;
}
