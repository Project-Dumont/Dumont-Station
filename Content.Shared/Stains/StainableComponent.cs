using Content.Shared.FixedPoint;
using Content.Shared.Hands.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Stains;

[RegisterComponent, NetworkedComponent]
public sealed partial class StainableComponent : Component
{
    [DataField]
    public string SolutionId = "stain";

    [DataField]
    public FixedPoint2 MaxVolume = 5f;

    [DataField]
    public FixedPoint2 StainVolume = 0.02f;

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new();

    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>> ItemVisuals = new();

    [DataField]
    public List<PrototypeLayerData> IconVisuals = new();

    [ViewVariables]
    public HashSet<int> RevealedIconVisuals = new();
}
