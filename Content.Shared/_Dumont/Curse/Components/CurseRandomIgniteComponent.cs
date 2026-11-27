using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dumont.Curse.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CurseRandomIgniteComponent : Component
{
    [DataField]
    public TimeSpan MinInterval = TimeSpan.FromMinutes(5);

    [DataField]
    public TimeSpan MaxInterval = TimeSpan.FromMinutes(15);

    [DataField]
    public float FireStacks = 2f;

    /// <summary>
    /// Dont ignite with more total damage than this.
    /// </summary>
    [DataField]
    public float MaxDamage = 50f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextIgnite;
}
