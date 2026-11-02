using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dumont.Curse.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CurseDropItemComponent : Component
{
    [DataField]
    public TimeSpan MinInterval = TimeSpan.FromMinutes(2);

    [DataField]
    public TimeSpan MaxInterval = TimeSpan.FromMinutes(6);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextDrop;
}
