using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Europa.AGPL.Server.CloningAppearance.Components;

[RegisterComponent]
public sealed partial class CloningAppearanceComponent : Component
{
    [DataField("components")]
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    [DataField("startingGear")]
    public ProtoId<StartingGearPrototype>? Gear;
}
