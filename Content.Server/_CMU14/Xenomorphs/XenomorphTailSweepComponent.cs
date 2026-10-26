using Content.Server._Dumont.Xenomorphs;
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphTailSweepComponent : Component
{
    [DataField] public float Range = 1.5f;
    [DataField] public int PlasmaCost = 10;
    [DataField] public DamageSpecifier Damage = new() { DamageDict = { ["Slash"] = 30 } };
    [DataField] public float KnockbackDistance = 1;
    [DataField] public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(2);
    [DataField] public SoundSpecifier Sound = new SoundCollectionSpecifier("XenoTailSwipe");
    [DataField] public SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_claw_block.ogg");
}
