namespace Content.Shared._Dumont.Power.Events;

[ByRefEvent]
public sealed class TryTogglePowerEvent : CancellableEntityEventArgs
{
    public EntityUid? User;

    public TryTogglePowerEvent(EntityUid? user)
    {
        User = user;
    }
}