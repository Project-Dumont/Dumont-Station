using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphBulwarkComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Encased;

    public EntityUid? ReflectEffect;

    [DataField, AutoNetworkedField]
    public bool Reflecting;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan ReflectExpiresAt;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan ReflectStartedAt;

    [DataField, AutoNetworkedField]
    public TimeSpan ReflectDuration = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public TimeSpan ReflectFullCooldown = TimeSpan.FromSeconds(18);

    [DataField, AutoNetworkedField]
    public TimeSpan ReflectMinCooldown = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public float ReflectCooldownPerSecond = 2f;

    [DataField, AutoNetworkedField]
    public EntProtoId ReflectEffectId = "DumontXenomorphReflectiveShieldEffect";

    [DataField, AutoNetworkedField]
    public TimeSpan TailSwingMissCooldown = TimeSpan.FromSeconds(0.3);

    [DataField, AutoNetworkedField]
    public SoundSpecifier TailSwingSound = new SoundPathSpecifier("/Audio/_CM13/Effects/tail_swing.wav");

    [DataField, AutoNetworkedField]
    public TimeSpan TailSwingParalyzeTime = TimeSpan.FromSeconds(1.2);

    [DataField, AutoNetworkedField]
    public float TailSwingRange = 1.75f;

    [DataField, AutoNetworkedField]
    public ProtoId<TagPrototype> TailSwingFlingable = "Grenade";

    [DataField, AutoNetworkedField]
    public float TailSwingFlingDistance = 3f;

    [DataField, AutoNetworkedField]
    public float TailSwingFlingSpeed = 10f;

    [DataField, AutoNetworkedField]
    public DamageSpecifier PlateBashDamage = new()
    {
        DamageDict = { ["Blunt"] = 20 },
    };

    [DataField, AutoNetworkedField]
    public TimeSpan PlateBashParalyzeTime = TimeSpan.FromSeconds(0.8);

    [DataField, AutoNetworkedField]
    public float PlateBashUnencasedKnockBackDistance = 1f;

    [DataField, AutoNetworkedField]
    public float PlateBashEncasedKnockBackDistance = 3f;

    [DataField, AutoNetworkedField]
    public float PlateBashEncasedRange = 1.5f;

    [DataField, AutoNetworkedField]
    public float PlateBashUnencasedLeapRange = 3f;

    [DataField, AutoNetworkedField]
    public float PlateBashUnencasedLeapSpeed = 30f;

    [DataField, AutoNetworkedField]
    public float PlateBashKnockBackSpeed = 8f;

    [DataField, AutoNetworkedField]
    public SoundSpecifier PlateBashSound = new SoundPathSpecifier("/Audio/_RMC14/Weapons/alien_knockdown.ogg");

    [DataField, AutoNetworkedField]
    public string[] EncasedImmuneToStatuses = { "KnockedDown" };

    [DataField, AutoNetworkedField]
    public int PassiveFrontalArmor = 10;

    [DataField, AutoNetworkedField]
    public int PassiveSideArmor = 10;

    [DataField, AutoNetworkedField]
    public int EncasedFrontalArmor = 20;

    [DataField, AutoNetworkedField]
    public int EncasedSideArmorPenalty = -10;

    [DataField, AutoNetworkedField]
    public float EncasedSpeedMultiplier = 0.68f;

    [DataField, AutoNetworkedField]
    public DamageSpecifier EncasedMeleePenalty = new()
    {
        DamageDict = { ["Slash"] = -8 },
    };
}

public sealed partial class XenomorphEncasedPlatesActionEvent : Content.Shared.Actions.InstantActionEvent;
public sealed partial class XenomorphPlateBashActionEvent : Content.Shared.Actions.EntityTargetActionEvent;
public sealed partial class XenomorphBulwarkTailSwingActionEvent : Content.Shared.Actions.InstantActionEvent;
public sealed partial class XenomorphReflectiveShieldActionEvent : Content.Shared.Actions.InstantActionEvent;
