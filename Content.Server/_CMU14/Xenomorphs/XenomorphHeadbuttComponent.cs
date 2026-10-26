using Content.Server._Dumont.Xenomorphs;
namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphHeadbuttComponent : Component
{
    [DataField] public float Range = 3.5f;
    [DataField] public int Damage = 30;
    [DataField] public int CrestedDamage = 20;
    [DataField] public float ThrowDistance = 1;
    [DataField] public float CrestedThrowDistance = 3;
    public bool Charging;
}
