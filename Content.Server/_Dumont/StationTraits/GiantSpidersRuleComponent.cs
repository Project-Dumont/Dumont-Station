using Content.Shared.Whitelist;

namespace Content.Server._Dumont.StationTraits;
[RegisterComponent]
public sealed partial class GiantSpidersRuleComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public float Scale = 1.2f;

    [DataField]
    public float Health = 1.2f;

    [DataField]
    public float Damage = 1.2f;
}
