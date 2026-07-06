using Robust.Shared.GameStates;
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
    public TimeSpan ShelfLife = TimeSpan.FromMinutes(20);

    /// <summary>
    /// How long this container has been actively spoiling
    [DataField]
    public TimeSpan SpoilAccumulator = TimeSpan.Zero;

    /// <summary>
    /// How spoiled this solution currently looks, from 0 (fresh)
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Stage;
}
