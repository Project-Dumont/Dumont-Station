using Content.Shared.Damage;

namespace Content.Server.Temperature.Components;

/// <summary>
/// If the atmos get too hot, it will damage the machine or it will explode
/// Requires <see cref="AtmosExposedComponent"/> to work and <see cref="ExplosiveComponent"/> if it will explode
/// </summary>
[RegisterComponent]
public sealed partial class OverheatMachineComponent : Component
{
    /// <summary>
    /// Temperature at which the machine start to be damaged. If null, the machine will not explode.
    /// </summary>
    [DataField]
    public float? HeatDamageTemperature;

    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Temperature at which the machine will explode. If null, the machine will not explode.
    /// </summary>
    [DataField]
    public float? ExplosionTemperature;

    [DataField]
    public float ExplosionRadius = 5f;

    /// <summary>
    /// Temperature at which the machine shuts down.
    /// </summary>
    [DataField]
    public float? TemperatureShutdown;
    
    [DataField]
    public bool IsShutdown;

}
