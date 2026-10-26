using Content.Server._Dumont.Xenomorphs;
namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphWallResinComponent : Component
{
    [DataField]
    public HashSet<EntityUid> Sources = new();

    [DataField]
    public EntityUid? Overlay;

    [DataField]
    public EntityUid? Nest;
}

[RegisterComponent]
public sealed partial class XenomorphWeedWallSpreadComponent : Component
{
    [DataField]
    public HashSet<EntityUid> Walls = new();
}
