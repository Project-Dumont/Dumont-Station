namespace Content.Goobstation.Shared.Gangs;

[RegisterComponent]
public sealed partial class GangLeaderComponent : Component
{
    [DataField]
    public EntityUid GangId;

    [DataField]
    public List<EntityUid> Members = new();
}
