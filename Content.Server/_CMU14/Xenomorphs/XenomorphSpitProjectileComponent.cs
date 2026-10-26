using Content.Server._Dumont.Xenomorphs;
using Robust.Shared.Map;
namespace Content.Server._CMU14.Xenomorphs;
[RegisterComponent]
public sealed partial class XenomorphSpitProjectileComponent : Component
{
    public bool Suppressed;
    [DataField] public bool DeleteOnFriendly = true;
    [DataField] public float SlowFactor = 0.8f;
    [DataField] public bool Charged;
    [DataField] public bool Acid;
    [DataField] public float Range = 5;
    [DataField] public TimeSpan Slow = TimeSpan.FromSeconds(8);
    [DataField] public TimeSpan Paralyze = TimeSpan.FromSeconds(3.5);
    [DataField] public bool ArmorResists = true;
    [DataField] public bool DrainMedicine = true;
    public MapCoordinates? Origin;
}
