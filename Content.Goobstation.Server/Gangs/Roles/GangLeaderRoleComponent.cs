namespace Content.Goobstation.Server.Gangs.Roles;

[RegisterComponent]
public sealed partial class GangLeaderRoleComponent : Component
{
    [DataField]
    public EntityUid GangId;
}
