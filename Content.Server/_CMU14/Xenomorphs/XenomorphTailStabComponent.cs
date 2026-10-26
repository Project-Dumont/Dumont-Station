using Content.Server._Dumont.Xenomorphs;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphTailStabComponent : Component
{
    [DataField] public float MissCooldown = 1;
    [DataField] public bool Corrosive;
    [DataField] public int AcidAmount = 2;
    [DataField] public float Range = 2.5f;
    [DataField] public DamageSpecifier Damage = new();
    [DataField] public EntProtoId HitEffect = "DumontXenomorphTailHit";
    [DataField] public SoundSpecifier HitSound = new SoundCollectionSpecifier("DumontXenomorphBite", AudioParams.Default.WithVariation(0.15f).WithVolume(-3));
    [DataField] public SoundSpecifier MissSound = new SoundCollectionSpecifier("XenoTailSwipe", AudioParams.Default.WithVariation(0.15f));
}
