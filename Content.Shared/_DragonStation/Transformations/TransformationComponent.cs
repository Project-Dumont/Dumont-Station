using System;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DragonStation.Transformations;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]

// Core tuning values for the transformation. We can later move these into reusable form configs.
public sealed partial class TransformationComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? ToggleAction = "ActionToggleSuperSaiyan";

    [DataField]
    public string ActivationPopup = "super-saiyan-transform-on";

    [DataField]
    public string DeactivationPopup = "super-saiyan-transform-off";

    [DataField]
    public string BlockedGearReason = "super-saiyan-cannot-equip-gear";

    [DataField]
    public string BlockedRangedPopup = "super-saiyan-cannot-use-ranged";

    [DataField]
    public string? RequiredFighterSkill;

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField, AutoNetworkedField]
    public bool IgnoreExternalStaminaDamage = true;

    [DataField, AutoNetworkedField]
    public bool SuppressStunEffects = true;

    [DataField, AutoNetworkedField]
    public bool StripBlockedGearOnTransform = true;

    [DataField, AutoNetworkedField]
    public bool BlockArmorSlots = true;

    [DataField, AutoNetworkedField]
    public bool BlockRangedWeapons = true;

    [DataField, AutoNetworkedField]
    public bool DropHeldGunsOnTransform = true;

    [DataField, AutoNetworkedField]
    public bool GrantsReflection = true;

    [DataField, AutoNetworkedField]
    public bool GrantsAura = true;

    [DataField, AutoNetworkedField]
    public float StaminaDrainRate = 2f;

    [DataField, AutoNetworkedField]
    public float SpeedModifier = 2.0f;

    [DataField, AutoNetworkedField]
    public float MeleeDamageModifier = 10.0f;

    [DataField, AutoNetworkedField]
    public float LowStaminaThreshold = 40f;

    [DataField]
    public TimeSpan RangedPopupCooldown = TimeSpan.FromSeconds(1f);

    [DataField]
    public TimeSpan NextRangedPopup;

    [DataField, AutoNetworkedField]
    public float ReflectProbability = 1f;

    [DataField, AutoNetworkedField]
    public Angle ReflectSpread = Angle.FromDegrees(60f);

    [DataField, AutoNetworkedField]
    public float BluntResistanceCoefficient = 0.7f;

    [DataField, AutoNetworkedField]
    public float SlashResistanceCoefficient = 0.7f;

    [DataField, AutoNetworkedField]
    public float PiercingResistanceCoefficient = 0.75f;

    [DataField, AutoNetworkedField]
    public float HeatResistanceCoefficient = 0.9f;

    [DataField, AutoNetworkedField]
    public float ColdResistanceCoefficient = 0.9f;

    public DamageModifierSet Resistances => new()
    {
        Coefficients =
        {
            ["Blunt"] = BluntResistanceCoefficient,
            ["Slash"] = SlashResistanceCoefficient,
            ["Piercing"] = PiercingResistanceCoefficient,
            ["Heat"] = HeatResistanceCoefficient,
            ["Cold"] = ColdResistanceCoefficient,
        }
    };
}
