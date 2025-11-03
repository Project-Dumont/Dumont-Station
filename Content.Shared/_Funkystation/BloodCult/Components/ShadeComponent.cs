using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// Spooky fella.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadeComponent : Component
{
	/// <summary>
	/// The soulstone that this shade was summoned from. The shade will return here on death.
	/// </summary>
	[DataField]
	public EntityUid? OriginSoulstone = null;
}
