namespace Content.Goobstation.Shared.Gangs;

[RegisterComponent]
public sealed partial class PendingGangHandshakeComponent : Component
{
    [DataField]
    public EntityUid Offerer;

    [DataField]
    public TimeSpan ExpiryTime;
}
