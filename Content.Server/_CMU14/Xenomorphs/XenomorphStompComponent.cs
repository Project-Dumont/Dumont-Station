using Content.Server._Dumont.Xenomorphs;
using Robust.Shared.Audio;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphStompComponent : Component
{
    [DataField] public float Range = 4;
    [DataField] public Angle Angle = Angle.FromDegrees(90);
    [DataField] public float Damage = 65;
    [DataField] public float MinDamage = 40;
    [DataField] public float PlasmaCost = 30;
    [DataField] public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(1);
    [DataField] public TimeSpan SlowTime = TimeSpan.FromSeconds(2.5);
    [DataField] public float KnockbackDistance = 2;
    [DataField] public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_footstep_charge1.ogg");
}
