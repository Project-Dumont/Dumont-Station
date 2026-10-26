using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphRavagerComponent : Component
{
    [DataField, AutoNetworkedField] public int EmpoweredTargets;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan SuperUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan? EmpowerAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan FirstActivation;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ShieldUntil;
    [DataField] public float Shield;
    public EntityUid? ShieldEffect;
    public EntityUid? SuperEffect;
}

[RegisterComponent]
public sealed partial class XenomorphRavagerObstacleComponent : Component
{
    [DataField] public bool Assault;
    [DataField] public bool Scissor;
    [DataField] public DamageSpecifier? ScissorDamage;
}

public sealed partial class XenomorphEmpowerActionEvent : InstantActionEvent;
public sealed partial class XenomorphScissorCutActionEvent : WorldTargetActionEvent;

[ByRefEvent]
public record struct XenomorphTailDamageEvent(DamageSpecifier Damage);
