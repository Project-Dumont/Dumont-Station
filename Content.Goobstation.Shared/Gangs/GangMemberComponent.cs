namespace Content.Goobstation.Shared.Gangs;

[RegisterComponent]
public sealed partial class GangMemberComponent : Component
{
    [DataField]
    public EntityUid GangId;
}
