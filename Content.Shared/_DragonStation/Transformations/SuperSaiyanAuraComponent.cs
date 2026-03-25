using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DragonStation.Transformations;

[RegisterComponent, NetworkedComponent]
public sealed partial class SuperSaiyanAuraComponent : Component
{
    [DataField]
    public SpriteSpecifier? Sprite = new SpriteSpecifier.Rsi(
        new("/Textures/DragonStation/Actions/Transformations/SuperSaiyan1_aura.rsi"),
        "ssj1aura_beta");

    [DataField]
    public Vector2 Offset = new(-0.03125f, 0.16875f);

    [DataField]
    public Vector2 Scale = new(0.65f, 0.65f);

    [DataField]
    public string Shader = "unshaded";
}

[Serializable, NetSerializable]
public enum SuperSaiyanAuraVisualKey
{
    Key
}
