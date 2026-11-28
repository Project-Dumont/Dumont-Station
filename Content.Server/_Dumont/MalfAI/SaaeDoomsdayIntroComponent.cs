using Content.Server._Funkystation.MalfAI;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Dumont.MalfAI;

/// <summary>
/// Added to a malf AI while the S.A.A.E. takeover plays out, before the doomsday countdown starts.
/// </summary>
[RegisterComponent, Access(typeof(MalfAiDoomsdaySystem))]
[AutoGenerateComponentPause]
public sealed partial class SaaeDoomsdayIntroComponent : Component
{
    /// <summary>
    /// Station the takeover is happening on.
    /// </summary>
    [DataField]
    public EntityUid Station;

    /// <summary>
    /// AI core holding the AI when the protocol was activated.
    /// </summary>
    [DataField]
    public EntityUid CoreHolder;

    /// <summary>
    /// When the protocol was activated, every step is timed from here.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan StartTime;

    /// <summary>
    /// Index of the next step of the takeover to run.
    /// </summary>
    [DataField]
    public int NextStep;

    /// <summary>
    /// The S.A.A.E. audio currently playing, so it can be cut if the protocol is aborted.
    /// </summary>
    [DataField]
    public EntityUid? Audio;
}
