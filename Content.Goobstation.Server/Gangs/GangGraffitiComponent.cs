using Robust.Shared.GameStates;

namespace Content.Goobstation.Server.Gangs;

[RegisterComponent]
public sealed partial class GangGraffitiComponent : Component
{
    [DataField]
    public EntityUid? GangId;
}
