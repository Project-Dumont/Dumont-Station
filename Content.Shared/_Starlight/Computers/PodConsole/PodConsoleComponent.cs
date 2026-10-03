using Robust.Shared.Audio; // Dumont
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Starlight.Computers.PodConsole;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class PodConsoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Locked = true;

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? LaunchTime;

    // Dumont changes start
    [DataField, AutoNetworkedField]
    public int? MaxOccupants;

    [DataField]
    public SoundSpecifier LaunchSound = new SoundPathSpecifier("/Audio/_Moffstation/Effects/Shuttle/hyperspace_begin.ogg", AudioParams.Default.WithVolume(-5f));

    [ViewVariables]
    public EntityUid? LaunchStream;
    // Dumont end
}
