using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.ChemicalSpoilage;

/// <summary>
/// Entities with this component slowly un-spoil any medicinal reagents that have already started rotting
/// also stops them from starting this effect
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReagentPreserverComponent : Component
{
    /// <summary>
    /// How much it reverts. 0 = no reverting.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ReversalRate = 0;
}
