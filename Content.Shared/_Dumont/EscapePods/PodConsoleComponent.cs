using Content.Shared.Radio;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dumont.EscapePods;

/// <summary>
/// Wall console inside an escape pod that launches it once unlocked.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class PodConsoleComponent : Component
{
    /// <summary>
    /// Locked consoles can't launch the pod.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Locked = true;

    /// <summary>
    /// When the pod leaves, null if no launch is pending.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? LaunchTime;

    /// <summary>
    /// How many people the pod can carry. When null the number of anchored seats on the pod is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int? MaxOccupants;

    /// <summary>
    /// How long someone has to hold the console to start the launch.
    /// </summary>
    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Time between finishing the launch on the console and the pod actually leaving.
    /// </summary>
    [DataField]
    public TimeSpan LaunchDelay = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Radio channel that gets told when the pod starts its countdown.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> LaunchChannel = "Common";

    /// <summary>
    /// How long before leaving the launch sound starts, so it ends right when the pod jumps.
    /// </summary>
    [DataField]
    public TimeSpan LaunchSoundLead = TimeSpan.FromSeconds(9);

    /// <summary>
    /// Sound played on the pod grid right before it leaves.
    /// </summary>
    [DataField]
    public SoundSpecifier LaunchSound = new SoundPathSpecifier("/Audio/_Moffstation/Effects/Shuttle/hyperspace_begin.ogg", AudioParams.Default.WithVolume(-5f));

    /// <summary>
    /// The launch sound that is playing, if any.
    /// </summary>
    [ViewVariables]
    public EntityUid? LaunchStream;
}
