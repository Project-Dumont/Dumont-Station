using Robust.Shared.GameStates;

namespace Content.Shared._Dumont.EscapePods;

/// <summary>
/// Lets a ninja hack an escape pod console to launch the pod right away with them inside.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedPodHackerSystem))]
public sealed partial class PodHackerComponent : Component
{
    /// <summary>
    /// Time taken to hack the console.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(20);
}
