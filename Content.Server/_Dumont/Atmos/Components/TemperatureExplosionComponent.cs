using Content.Shared.Damage;

namespace Content.Server.Temperature.Components;

[RegisterComponent]
public sealed partial class TemperatureExplosionComponent : Component
{
    [DataField]
    public float HeatDamageTemperature;

    [DataField]
    public DamageSpecifier HeatDamage = new();

    [DataField]
    public float ExplosionTemperature;

    [DataField]
    public float ExplosionRadius;
    
}
