using System.Linq;
using Content.Goobstation.Shared.MartialArts;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Server._DragonStation.FighterProgression;
using Content.Server.Dragon;
using Content.Server.Roles;
using Content.Shared.Roles;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._DragonStation.FighterProgression;
using Content.Shared._DragonStation.FighterProgression.Prototypes;
using Content.Shared._DragonStation.PowerLevel;
using Content.Shared._DragonStation.Transformations;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DragonStation.FighterProgression;

public sealed class FighterProgressionSystem : SharedFighterProgressionSystem
{
    [Dependency] private readonly SharedMartialArtsSystem _martialArts = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private static readonly ProtoId<TagPrototype> CarpTag = "Carp";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<FighterProgressionComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<FighterProgressionComponent, FighterChooseBranchMessage>(OnChooseBranch);
        SubscribeLocalEvent<FighterProgressionComponent, PowerLevelRefreshRequestedEvent>(OnPowerLevelRefreshRequested);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (args.JobId is not ("Boxer" or "VisitorBoxer"))
            return;

        EnsureComp<FighterProgressionComponent>(args.Mob);
    }

    private void OnMeleeHit(MeleeHitEvent args)
    {
        if (!TryComp<FighterProgressionComponent>(args.User, out var component))
            return;

        var user = args.User;
        if (!args.IsHit || !IsFighterStyleWeapon(user, args.Weapon))
            return;

        var awardedCombat = false;
        var awardedTraining = false;

        foreach (var hit in args.HitEntities.Distinct())
        {
            if (hit == user)
                continue;

            ProcessChallengeHit(user, component, hit);

            if (HasComp<FighterTrainingTargetComponent>(hit))
            {
                if (_timing.CurTime < component.NextTrainingXpTime)
                    continue;

                component.NextTrainingXpTime = _timing.CurTime + component.TrainingHitCooldown;
                AddXp(user, component, component.TrainingHitXp);
                awardedTraining = true;
                continue;
            }

            if (!HasComp<MobStateComponent>(hit))
                continue;

            AddXp(user, component, component.CombatHitXp);
            awardedCombat = true;
        }

        if (awardedCombat || awardedTraining)
            UpdateUi(user, component);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.Origin == null)
            return;

        var query = EntityQueryEnumerator<FighterProgressionComponent>();
        while (query.MoveNext(out var fighterUid, out var component))
        {
            if (args.Origin != fighterUid)
                continue;

            RefreshMissedChallenges(component);
            TryCompleteKillChallenge(fighterUid, component, args.Target);
        }
    }

    private void AddXp(EntityUid uid, FighterProgressionComponent component, int amount)
    {
        if (amount <= 0)
            return;

        component.CurrentXp += amount;
        ProcessThresholdAdvancement(uid, component);
        Dirty(uid, component);
    }

    private void OnChooseBranch(EntityUid uid, FighterProgressionComponent component, FighterChooseBranchMessage args)
    {
        if (!_prototype.TryIndex(args.SkillId, out FighterSkillPrototype? skill))
            return;

        var pendingOptions = GetPendingChoiceOptions(uid, component);
        if (!pendingOptions.Contains(skill.ID))
            return;

        var availability = GetAvailability(uid, skill, component);
        if (availability != FighterSkillAvailability.BranchChoiceAvailable)
            return;

        foreach (var option in pendingOptions)
        {
            if (option == skill.ID)
                continue;

            if (!component.ClosedSkills.Contains(option))
                component.ClosedSkills.Add(option);
        }

        foreach (var sibling in _prototype.EnumeratePrototypes<FighterSkillPrototype>())
        {
            if (sibling.ID == skill.ID)
                continue;

            if (!HaveMatchingPrerequisites(sibling, skill))
                continue;

            if (!component.ClosedSkills.Contains(sibling.ID))
                component.ClosedSkills.Add(sibling.ID);
        }

        component.PendingChoiceOptions.Clear();
        UnlockSkill(uid, component, skill);
        ProcessThresholdAdvancement(uid, component);
        Dirty(uid, component);
        UpdateUi(uid, component);
    }

    private void OnUiOpened(EntityUid uid, FighterProgressionComponent component, BoundUIOpenedEvent args)
    {
        UpdateUi(uid, component);
    }

    private void OnPowerLevelRefreshRequested(EntityUid uid, FighterProgressionComponent component, PowerLevelRefreshRequestedEvent args)
    {
        UpdateUi(uid, component);
    }

    private void UpdateUi(EntityUid uid, FighterProgressionComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        RefreshMissedChallenges(component);

        var states = new List<FighterSkillState>();

        foreach (var skill in _prototype.EnumeratePrototypes<FighterSkillPrototype>())
        {
            states.Add(new FighterSkillState(skill.ID, GetAvailability(uid, skill, component)));
        }

        var powerLevel = CompOrNull<PowerLevelComponent>(uid)?.CurrentPowerLevel ?? 100;

        var state = new FighterSkillTreeBoundUserInterfaceState(
            component.ThresholdsReached,
            powerLevel,
            component.CurrentXp,
            component.XpThreshold,
            component.PendingChoiceOptions.Count > 0,
            states);

        _ui.SetUiState(uid, FighterSkillTreeUiKey.Key, state);
    }

    private void ProcessThresholdAdvancement(EntityUid uid, FighterProgressionComponent component)
    {
        while (component.CurrentXp >= component.XpThreshold)
        {
            RefreshMissedChallenges(component);

            if (component.PendingChoiceOptions.Count > 0)
                break;

            var eligible = GetEligibleSkills(component).ToList();
            if (eligible.Count == 0)
                break;

            component.CurrentXp -= component.XpThreshold;
            component.ThresholdsReached++;

            if (eligible.Count == 1)
            {
                UnlockSkill(uid, component, eligible[0], "fighter-progression-threshold-earned");
                continue;
            }

            component.PendingChoiceOptions.Clear();
            foreach (var skill in eligible)
            {
                component.PendingChoiceOptions.Add(skill.ID);
            }
            _popup.PopupEntity(Loc.GetString("fighter-progression-branch-choice-ready"), uid, uid);
            break;
        }
    }

    private void UnlockSkill(EntityUid uid, FighterProgressionComponent component, FighterSkillPrototype skill, string popupLoc = "fighter-progression-skill-unlocked")
    {
        if (component.UnlockedSkills.Contains(skill.ID))
            return;

        component.UnlockedSkills.Add(skill.ID);
        component.MissedChallengeSkills.Remove(skill.ID);
        component.ChallengeProgress.Remove(skill.ID);
        RefreshMissedChallenges(component);

        // SSJ unlocks the transformation component the first time the awakening node is reached.
        if (skill.ID == "FighterGoldenWarriorAwakening")
            EnsureComp<TransformationComponent>(uid);

        if (skill.MartialArtUnlock != null)
            _martialArts.TryGrantMartialArtKnowledge(uid, skill.MartialArtUnlock.Value);

        RaiseLocalEvent(uid, new FighterProgressionChangedEvent(), true);
        RaiseLocalEvent(uid, new PowerLevelRefreshRequestedEvent(), true);
        _popup.PopupEntity(Loc.GetString(popupLoc, ("skill", Loc.GetString(skill.Name))), uid, uid);
    }

    private void ProcessChallengeHit(EntityUid uid, FighterProgressionComponent component, EntityUid target)
    {
        RefreshMissedChallenges(component);

        foreach (var skill in GetActiveChallengeSkills(uid, component))
        {
            if (skill.ChallengeTarget == null || !MatchesChallengeTarget(target, skill.ChallengeTarget.Value))
                continue;

            var progress = GetChallengeProgress(component, skill.ID);
            if (progress.Target != target)
            {
                progress.Target = target;
                progress.Hits = 0;
            }

            progress.Hits++;

            if (!skill.ChallengeRequiresKill && progress.Hits >= skill.ChallengeRequiredHits)
            {
                UnlockSkill(uid, component, skill);
                UpdateUi(uid, component);
            }
        }
    }

    private void TryCompleteKillChallenge(EntityUid uid, FighterProgressionComponent component, EntityUid target)
    {
        foreach (var skill in GetActiveChallengeSkills(uid, component))
        {
            if (!skill.ChallengeRequiresKill || skill.ChallengeTarget == null)
                continue;

            if (!MatchesChallengeTarget(target, skill.ChallengeTarget.Value))
                continue;

            if (!component.ChallengeProgress.TryGetValue(skill.ID, out var progress))
                continue;

            if (progress.Target != target || progress.Hits < skill.ChallengeRequiredHits)
                continue;

            UnlockSkill(uid, component, skill);
            UpdateUi(uid, component);
        }
    }

    private FighterChallengeProgress GetChallengeProgress(FighterProgressionComponent component, string skillId)
    {
        if (!component.ChallengeProgress.TryGetValue(skillId, out var progress))
        {
            progress = new FighterChallengeProgress();
            component.ChallengeProgress[skillId] = progress;
        }

        return progress;
    }

    private bool MatchesChallengeTarget(EntityUid target, FighterSkillChallengeTarget challengeTarget)
    {
        return challengeTarget switch
        {
            FighterSkillChallengeTarget.Carp => _tag.HasTag(target, CarpTag),
            FighterSkillChallengeTarget.Bear => MetaData(target).EntityPrototype?.ID is "MobBearSpace" or "MobBearSpaceSalvage",
            FighterSkillChallengeTarget.Borg => HasComp<BorgChassisComponent>(target),
            FighterSkillChallengeTarget.Dragon => HasComp<DragonComponent>(target),
            _ => false,
        };
    }

    protected override bool MeetsSpecialRequirements(EntityUid uid, FighterSkillPrototype skill, FighterProgressionComponent component)
    {
        if (!skill.RequiresTraitorRole)
            return true;

        if (!_mind.TryGetMind(uid, out var mindId, out _))
            return false;

        return _roles.MindHasRole<TraitorRoleComponent>(mindId);
    }
}
