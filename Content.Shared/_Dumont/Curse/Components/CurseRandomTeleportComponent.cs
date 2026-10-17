using Content.Shared.Teleportation;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dumont.Curse.Components;

/// <summary>
/// Every x times, teleports to a random nearby spot.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class CurseRandomTeleportComponent : RandomTeleportComponent
{
    [DataField]
    public TimeSpan MinInterval = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan MaxInterval = TimeSpan.FromMinutes(10);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextTeleport;
}
