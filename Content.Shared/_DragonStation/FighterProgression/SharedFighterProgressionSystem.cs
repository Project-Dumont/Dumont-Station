using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared._DragonStation.FighterProgression.Prototypes;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._DragonStation.FighterProgression;

public abstract class SharedFighterProgressionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] protected readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FighterProgressionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FighterProgressionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FighterProgressionComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<FighterProgressionComponent, GetUserMeleeDamageEvent>(OnGetUserMeleeDamage);
        SubscribeLocalEvent<FighterProgressionComponent, GetMeleeAttackRateEvent>(OnGetMeleeAttackRate);
        SubscribeLocalEvent<FighterProgressionComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<FighterProgressionComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnStartup(EntityUid uid, FighterProgressionComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.OpenTreeActionEntity, component.OpenTreeAction);
    }

    private void OnShutdown(EntityUid uid, FighterProgressionComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.OpenTreeActionEntity);
    }

    private void OnRefreshMovementSpeed(EntityUid uid, FighterProgressionComponent component, ref RefreshMovementSpeedModifiersEvent args)
    {
        var bonuses = GetBonuses(uid, component);
        if (bonuses.PassiveSpeedMultiplier <= 1f)
            return;

        args.ModifySpeed(bonuses.PassiveSpeedMultiplier, bonuses.PassiveSpeedMultiplier);
    }

    private void OnGetUserMeleeDamage(EntityUid uid, FighterProgressionComponent component, ref GetUserMeleeDamageEvent args)
    {
        var bonuses = GetBonuses(uid, component);
        if (bonuses.PassiveMeleeMultiplier <= 1f || !IsFighterStyleWeapon(uid, args.Weapon))
            return;

        args.Damage *= bonuses.PassiveMeleeMultiplier;
    }

    private void OnGetMeleeAttackRate(EntityUid uid, FighterProgressionComponent component, ref GetMeleeAttackRateEvent args)
    {
        var bonuses = GetBonuses(uid, component);
        if (bonuses.PassiveUnarmedAttackSpeedMultiplier <= 1f || !IsFighterStyleWeapon(uid, args.Weapon))
            return;

        args.Multipliers *= bonuses.PassiveUnarmedAttackSpeedMultiplier;
    }

    private void OnBeforeStaminaDamage(EntityUid uid, FighterProgressionComponent component, ref BeforeStaminaDamageEvent args)
    {
        var bonuses = GetBonuses(uid, component);
        if (bonuses.PassiveStaminaDrainMultiplier >= 1f)
            return;

        if (args.Value <= 0f || args.Source != uid)
            return;

        args.Value *= bonuses.PassiveStaminaDrainMultiplier;
    }

    private void OnDamageModify(EntityUid uid, FighterProgressionComponent component, ref DamageModifyEvent args)
    {
        var bonuses = GetBonuses(uid, component);
        if (bonuses.PassivePhysicalResistanceCoefficientMultiplier >= 1f &&
            bonuses.PassiveTemperatureResistanceCoefficientMultiplier >= 1f)
            return;

        var resistances = new DamageModifierSet
        {
            Coefficients =
            {
                ["Blunt"] = bonuses.PassivePhysicalResistanceCoefficientMultiplier,
                ["Slash"] = bonuses.PassivePhysicalResistanceCoefficientMultiplier,
                ["Piercing"] = bonuses.PassivePhysicalResistanceCoefficientMultiplier,
                ["Heat"] = bonuses.PassiveTemperatureResistanceCoefficientMultiplier,
                ["Cold"] = bonuses.PassiveTemperatureResistanceCoefficientMultiplier,
            }
        };

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage,
            DamageSpecifier.PenetrateArmor(resistances, args.Damage.ArmorPenetration));
    }

    public bool HasSkill(EntityUid uid, string skillId, FighterProgressionComponent? component = null)
    {
        return Resolve(uid, ref component, false) && component.UnlockedSkills.Any(id => id == skillId);
    }

    public bool HasPendingBranchChoice(EntityUid uid, FighterProgressionComponent? component = null)
    {
        return Resolve(uid, ref component, false) && component.PendingChoiceOptions.Count > 0;
    }

    public FighterSkillAvailability GetAvailability(EntityUid uid, FighterSkillPrototype skill, FighterProgressionComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return FighterSkillAvailability.Locked;

        RefreshMissedChallenges(component);

        if (skill.SpecialChallengeUnlock && !IsChallengeVisible(uid, skill, component))
            return FighterSkillAvailability.Hidden;

        if (component.UnlockedSkills.Contains(skill.ID))
            return FighterSkillAvailability.Unlocked;

        if (IsSkillBlockedByBranchChoice(skill.ID, component))
            return FighterSkillAvailability.BlockedByBranchChoice;

        if (skill.Prerequisites.All(component.UnlockedSkills.Contains) &&
            !MeetsSpecialRequirements(uid, skill, component))
            return FighterSkillAvailability.RequirementLocked;

        if (GetPendingChoiceOptions(uid, component).Contains(skill.ID))
            return FighterSkillAvailability.BranchChoiceAvailable;

        var nextAutoUnlock = GetNextAutoUnlock(uid, component);
        if (nextAutoUnlock == skill.ID)
            return FighterSkillAvailability.NextAutoUnlock;

        return FighterSkillAvailability.Locked;
    }

    public ProtoId<FighterSkillPrototype>? GetNextAutoUnlock(EntityUid uid, FighterProgressionComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return null;

        if (component.PendingChoiceOptions.Count > 0)
            return null;

        var eligible = GetEligibleSkills(component).ToList();
        if (eligible.Count != 1)
            return null;

        return eligible[0].ID;
    }

    protected HashSet<ProtoId<FighterSkillPrototype>> GetPendingChoiceOptions(EntityUid uid, FighterProgressionComponent component)
    {
        var options = new HashSet<ProtoId<FighterSkillPrototype>>();

        if (component.PendingChoiceOptions.Count == 0)
            return options;

        FighterSkillPrototype? seed = null;
        foreach (var option in component.PendingChoiceOptions)
        {
            if (!_prototype.TryIndex(option, out FighterSkillPrototype? candidate))
                continue;

            seed = candidate;
            break;
        }

        if (seed == null)
            return options;

        foreach (var skill in _prototype.EnumeratePrototypes<FighterSkillPrototype>())
        {
            if (component.UnlockedSkills.Contains(skill.ID))
                continue;

            if (skill.SpecialChallengeUnlock)
                continue;

            if (IsSkillBlockedByBranchChoice(skill.ID, component))
                continue;

            if (!HaveMatchingPrerequisites(skill, seed))
                continue;

            if (!skill.Prerequisites.All(component.UnlockedSkills.Contains))
                continue;

            if (!MeetsSpecialRequirements(uid, skill, component))
                continue;

            options.Add(skill.ID);
        }

        return options;
    }

    protected IEnumerable<FighterSkillPrototype> GetEligibleSkills(FighterProgressionComponent component)
    {
        RefreshMissedChallenges(component);

        return _prototype.EnumeratePrototypes<FighterSkillPrototype>()
            .Where(skill => !component.UnlockedSkills.Contains(skill.ID))
            .Where(skill => !skill.SpecialChallengeUnlock)
            .Where(skill => !IsSkillBlockedByBranchChoice(skill.ID, component))
            .Where(skill => skill.Prerequisites.All(component.UnlockedSkills.Contains))
            .Where(skill => MeetsSpecialRequirements(component.Owner, skill, component))
            .OrderBy(skill => skill.Position.X)
            .ThenBy(skill => skill.Position.Y);
    }

    protected virtual bool MeetsSpecialRequirements(EntityUid uid, FighterSkillPrototype skill, FighterProgressionComponent component)
    {
        return true;
    }

    protected bool IsChallengeVisible(EntityUid uid, FighterSkillPrototype skill, FighterProgressionComponent component)
    {
        if (!skill.SpecialChallengeUnlock)
            return true;

        if (component.UnlockedSkills.Contains(skill.ID))
            return true;

        if (component.MissedChallengeSkills.Contains(skill.ID))
            return false;

        foreach (var prerequisite in skill.Prerequisites)
        {
            if (!_prototype.TryIndex(prerequisite, out FighterSkillPrototype? prerequisiteSkill))
                continue;

            if (!prerequisiteSkill.SpecialChallengeUnlock)
                continue;

            return component.UnlockedSkills.Contains(prerequisiteSkill.ID);
        }

        return false;
    }

    protected bool IsChallengeActive(EntityUid uid, FighterSkillPrototype skill, FighterProgressionComponent component)
    {
        if (!skill.SpecialChallengeUnlock)
            return false;

        if (component.UnlockedSkills.Contains(skill.ID) || component.MissedChallengeSkills.Contains(skill.ID))
            return false;

        if (skill.ChallengeParentSkill == null || !component.UnlockedSkills.Contains(skill.ChallengeParentSkill.Value))
            return false;

        if (skill.ChallengeExpiresAfterSkill != null && component.UnlockedSkills.Contains(skill.ChallengeExpiresAfterSkill.Value))
            return false;

        return skill.Prerequisites.All(prereq => component.UnlockedSkills.Contains(prereq));
    }

    protected IEnumerable<FighterSkillPrototype> GetActiveChallengeSkills(EntityUid uid, FighterProgressionComponent component)
    {
        RefreshMissedChallenges(component);

        return _prototype.EnumeratePrototypes<FighterSkillPrototype>()
            .Where(skill => skill.SpecialChallengeUnlock)
            .Where(skill => IsChallengeActive(uid, skill, component))
            .OrderBy(skill => skill.Position.X)
            .ThenBy(skill => skill.Position.Y);
    }

    protected void RefreshMissedChallenges(FighterProgressionComponent component)
    {
        foreach (var skill in _prototype.EnumeratePrototypes<FighterSkillPrototype>())
        {
            if (!skill.SpecialChallengeUnlock || skill.ChallengeExpiresAfterSkill == null)
                continue;

            if (component.UnlockedSkills.Contains(skill.ID))
                continue;

            if (component.UnlockedSkills.Contains(skill.ChallengeExpiresAfterSkill.Value))
                component.MissedChallengeSkills.Add(skill.ID);
        }
    }

    protected bool IsSkillBlockedByBranchChoice(ProtoId<FighterSkillPrototype> skillId, FighterProgressionComponent component)
    {
        return IsSkillBlockedByBranchChoice(skillId, component, new HashSet<string>());
    }

    private bool IsSkillBlockedByBranchChoice(ProtoId<FighterSkillPrototype> skillId, FighterProgressionComponent component, HashSet<string> visited)
    {
        if (!visited.Add(skillId))
            return false;

        if (component.ClosedSkills.Contains(skillId))
            return true;

        if (!_prototype.TryIndex(skillId, out FighterSkillPrototype? skill))
            return false;

        return skill.Prerequisites.Any(prereq => IsSkillBlockedByBranchChoice(prereq, component, visited));
    }

    protected static bool HaveMatchingPrerequisites(FighterSkillPrototype left, FighterSkillPrototype right)
    {
        if (left.Prerequisites.Count != right.Prerequisites.Count)
            return false;

        return left.Prerequisites.Order().SequenceEqual(right.Prerequisites.Order());
    }

    protected bool IsFighterStyleWeapon(EntityUid uid, EntityUid weapon)
    {
        if (weapon == uid)
            return true;

        return _inventory.TryGetSlotEntity(uid, "gloves", out var gloves) && gloves == weapon;
    }

    public FighterProgressionBonuses GetBonuses(EntityUid uid, FighterProgressionComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return FighterProgressionBonuses.Default;

        var bonuses = FighterProgressionBonuses.Default;

        foreach (var skillId in component.UnlockedSkills)
        {
            if (!_prototype.TryIndex(skillId, out FighterSkillPrototype? skill))
                continue;

            bonuses.PassiveMeleeMultiplier += skill.PassiveMeleeBonus;
            bonuses.PassiveSpeedMultiplier += skill.PassiveSpeedBonus;
            bonuses.PassiveStaminaDrainMultiplier *= skill.PassiveStaminaDrainMultiplier;
            bonuses.PassiveUnarmedAttackSpeedMultiplier += skill.PassiveUnarmedAttackSpeedBonus;
            bonuses.PassivePhysicalResistanceCoefficientMultiplier *= skill.PassivePhysicalResistanceCoefficientMultiplier;
            bonuses.PassiveTemperatureResistanceCoefficientMultiplier *= skill.PassiveTemperatureResistanceCoefficientMultiplier;
            bonuses.TransformationMeleeMultiplier += skill.TransformationMeleeBonus;
            bonuses.TransformationSpeedMultiplier += skill.TransformationSpeedBonus;
            bonuses.TransformationStaminaDrainMultiplier *= skill.TransformationStaminaDrainMultiplier;
            bonuses.TransformationResistanceCoefficientMultiplier *= skill.TransformationResistanceCoefficientMultiplier;
        }

        return bonuses;
    }
}

public sealed class FighterProgressionChangedEvent : EntityEventArgs;

public record struct FighterProgressionBonuses(
    float PassiveMeleeMultiplier,
    float PassiveSpeedMultiplier,
    float PassiveStaminaDrainMultiplier,
    float PassiveUnarmedAttackSpeedMultiplier,
    float PassivePhysicalResistanceCoefficientMultiplier,
    float PassiveTemperatureResistanceCoefficientMultiplier,
    float TransformationMeleeMultiplier,
    float TransformationSpeedMultiplier,
    float TransformationStaminaDrainMultiplier,
    float TransformationResistanceCoefficientMultiplier)
{
    public static readonly FighterProgressionBonuses Default = new(
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f);
}
