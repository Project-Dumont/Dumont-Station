using System;
using Content.Goobstation.Common.MartialArts;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._DragonStation.FighterProgression.Prototypes;

[Serializable]
public enum FighterSkillChallengeTarget
{
    Carp,
    Bear,
    Borg,
    Dragon,
}

[DataDefinition]
[Prototype("fighterSkill")]
public sealed partial class FighterSkillPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name = string.Empty;

    [DataField(required: true)]
    public LocId Description = string.Empty;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public ProtoId<FighterBranchPrototype> Branch;

    [DataField]
    public List<ProtoId<FighterSkillPrototype>> Prerequisites = new();

    [DataField(required: true)]
    public Vector2i Position;

    [DataField]
    public List<LocId> EffectDescriptions = new();

    [DataField]
    public float PassiveMeleeBonus;

    [DataField]
    public float PassiveSpeedBonus;

    [DataField]
    public float PassiveStaminaDrainMultiplier = 1f;

    [DataField]
    public float PassiveUnarmedAttackSpeedBonus;

    [DataField]
    public float PassivePhysicalResistanceCoefficientMultiplier = 1f;

    [DataField]
    public float PassiveTemperatureResistanceCoefficientMultiplier = 1f;

    [DataField]
    public float TransformationMeleeBonus;

    [DataField]
    public float TransformationSpeedBonus;

    [DataField]
    public float TransformationStaminaDrainMultiplier = 1f;

    [DataField]
    public float TransformationResistanceCoefficientMultiplier = 1f;

    [DataField]
    public MartialArtsForms? MartialArtUnlock;

    [DataField]
    public bool RequiresTraitorRole;

    [DataField]
    public bool SpecialChallengeUnlock;

    [DataField]
    public ProtoId<FighterSkillPrototype>? ChallengeParentSkill;

    [DataField]
    public ProtoId<FighterSkillPrototype>? ChallengeExpiresAfterSkill;

    [DataField]
    public FighterSkillChallengeTarget? ChallengeTarget;

    [DataField]
    public int ChallengeRequiredHits;

    [DataField]
    public bool ChallengeRequiresKill;
}
