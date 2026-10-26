using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Curse.Components;

/// <summary>
/// Puts an armed bomb in the chest cavity.
/// </summary>
[RegisterComponent]
public sealed partial class CurseCavityBombComponent : Component
{
    [DataField]
    public float RealChance = 0.5f;

    [DataField]
    public EntProtoId RealPrototype = "CurseC4";

    [DataField]
    public EntProtoId FakePrototype = "CurseC4Dud";
}
