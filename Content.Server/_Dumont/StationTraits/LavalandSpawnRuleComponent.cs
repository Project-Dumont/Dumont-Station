using Robust.Shared.Map;

namespace Content.Server._Dumont.StationTraits;
[RegisterComponent]
public sealed partial class LavalandSpawnRuleComponent : Component
{
    [ViewVariables]
    public List<EntityCoordinates>? Tiles;
}
