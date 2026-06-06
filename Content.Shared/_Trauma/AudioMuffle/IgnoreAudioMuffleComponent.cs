using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.AudioMuffle;

/// <summary>
/// Ignore audio muffle.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IgnoreAudioMuffleComponent : Component;
