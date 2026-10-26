using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphCrusherComponent : Component
{
    [DataField] public float ChargeRange = 5;
    [DataField] public float ChargeDamage = 65;
    [DataField] public float StructuralDamage = 750;
    [DataField] public TimeSpan ChargeDelay = TimeSpan.FromSeconds(1.2);
    [DataField] public float SoakDamageGoal = 240;
    [DataField] public float SoakHeal = 120;
    [DataField] public TimeSpan SoakDuration = TimeSpan.FromSeconds(6);
    [DataField] public float ShieldAmount = 100;
    [DataField] public float ShieldReduction = 8;
    [DataField] public TimeSpan ShieldDuration = TimeSpan.FromSeconds(7);
    [DataField] public TimeSpan ExplosionDuration = TimeSpan.FromSeconds(2.5);
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ChargeAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan SoakUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ShieldUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ExplosionUntil;
    public EntityCoordinates? ChargeTarget;
    public Vector2 ChargeOrigin;
    public Vector2 ChargeDirection;
    public bool Charging;
    public float SoakAccumulated;
    public float ShieldRemaining;
}
