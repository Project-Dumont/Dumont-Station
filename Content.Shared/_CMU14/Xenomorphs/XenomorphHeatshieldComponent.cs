using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphHeatshieldComponent : Component
{
    [DataField] public float FireDamageMultiplier = 0.5f;
    [DataField] public DamageSpecifier BurningMeleeDamage = new() { DamageDict = { ["Heat"] = 3 } };
    [DataField] public float SpeedMultiplier = 1.35f;
    [DataField] public float AttackRateMultiplier = 1.2f;
    [DataField, AutoNetworkedField] public bool Thermoregulating;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan ExpiresAt;
}

public sealed partial class XenomorphSelfImmolateActionEvent : InstantActionEvent;
public sealed partial class XenomorphThermoregulationActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphSelfImmolateDoAfterEvent : SimpleDoAfterEvent;
