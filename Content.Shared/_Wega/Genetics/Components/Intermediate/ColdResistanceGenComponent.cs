using Content.Shared.Damage;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class ColdResistanceGenComponent : Component
{
    public float OldColdResistance;

    public bool RemBarotrauma = false;

    public DamageSpecifier OldDamage = default!;
}
