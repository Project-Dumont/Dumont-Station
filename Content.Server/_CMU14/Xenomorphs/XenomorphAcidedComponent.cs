using Content.Server._Dumont.Xenomorphs;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
namespace Content.Server._CMU14.Xenomorphs;
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphAcidedComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ExpiresAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextDamageAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextWashAt;
    public int Tier = 1;
    public int FinishingTier;
    public float DamagePerTick = 1;
    public TimeSpan Duration = TimeSpan.FromSeconds(20);
    public float ArmorPenetration = .4f;
    public bool Structure;
    public bool Combo;
    public int Resists = 1;
    public EntityUid? Effect;
}
