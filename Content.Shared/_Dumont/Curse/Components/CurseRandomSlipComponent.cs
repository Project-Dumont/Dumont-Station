using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dumont.Curse.Components;

/// <summary>
/// Mkaes you slips on nothing.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CurseRandomSlipComponent : Component
{
    [DataField]
    public TimeSpan MinInterval = TimeSpan.FromMinutes(1);

    [DataField]
    public TimeSpan MaxInterval = TimeSpan.FromMinutes(8);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextSlip;
}
