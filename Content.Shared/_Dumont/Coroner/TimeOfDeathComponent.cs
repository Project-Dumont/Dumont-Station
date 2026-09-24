using Robust.Shared.GameStates;

namespace Content.Shared._Dumont.Coroner;

[RegisterComponent, NetworkedComponent]
public sealed partial class TimeOfDeathComponent : Component
{
    [DataField]
    public TimeSpan Time;
}
