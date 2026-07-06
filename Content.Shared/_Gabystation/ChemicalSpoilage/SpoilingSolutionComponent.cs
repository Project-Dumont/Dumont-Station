using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Gabystation.ChemicalSpoilage;

/// <summary>
/// Tracks rotting progress for a solution that contains reagents from the
/// <code>Medicine</code> metabolism group.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedChemicalSpoilageSystem))]
public sealed partial class SpoilingSolutionComponent : Component
{
    /// <summary>
    /// The name of the solution being tracked, e.g. "beaker".
    /// </summary>
    [DataField]
    public string Solution = "beaker";


    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long it takes a fully fresh medicinal reagent to become fully spoiled if left unattended
    /// </summary>
    [DataField]
    public TimeSpan ShelfLife = TimeSpan.FromMinutes(5);

    /// <summary>
    /// How spoiled this solution currently looks, from 0 (fresh)
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Stage;

    /// <summary>
    /// Tracks how much of each original reagent has been converted into toxin due to spoilage
    /// </summary>
    [DataField]
    public List<SpoiledPortion> Ledger = new();
}

/// <summary>
/// A track of how much of a specific reagent has been converted into Toxin by spoilage.
/// </summary>
[DataDefinition]
public sealed partial class SpoiledPortion
{
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent = default!;

    [DataField(required: true)]
    public FixedPoint2 Quantity;

    public SpoiledPortion()
    {
    }

    public SpoiledPortion(ProtoId<ReagentPrototype> reagent, FixedPoint2 quantity)
    {
        Reagent = reagent;
        Quantity = quantity;
    }
}
