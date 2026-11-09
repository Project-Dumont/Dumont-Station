// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._CMU14.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Robust.Shared.Audio;
using Content.Server._CMU14.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Server.Body.Systems;
using Content.Server.Mind;
using Content.Server._White.Xenomorphs.Infection;
using Content.Shared.Mind.Components;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Shared._White.Xenomorphs.Infection;
using Content.Shared.Body.Components; // Goobstation start
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared._White.Xenomorphs.FaceHugger;
using Content.Shared.Mobs.Components;
using Content.Shared.Throwing;
using Content.Shared.Atmos.Components;
using Content.Shared.Nutrition.Components; // Goobstation end
using Content.Goobstation.Shared.Xenomorph; // Omu

namespace Content.Server._White.Xenomorphs.FaceHugger;

public sealed partial class FaceHuggerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ReactiveSystem _reactiveSystem = default!; // Goobstation
    [Dependency] private SharedSolutionContainerSystem _solutions = default!; // Goobstation

    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private ContainerSystem _container = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private EntityLookupSystem _entityLookup = default!;
    [Dependency] private EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private StunSystem _stun = default!;

    // Dumont start
    [Dependency] private MindSystem _mind = default!;
    // Dumont end

    private readonly Queue<EntityUid> _readyToInfect = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaceHuggerComponent, StartCollideEvent>(OnCollideEvent);
        SubscribeLocalEvent<FaceHuggerComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<FaceHuggerComponent, GotEquippedHandEvent>(OnPickedUp);
        SubscribeLocalEvent<FaceHuggerComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<FaceHuggerComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<FaceHuggerComponent, GotUnequippedEvent>(OnGotUnequipped);
        SubscribeLocalEvent<FaceHuggerComponent, FaceHuggerProtectionDoAfterEvent>(OnProtectionFinished);
        SubscribeLocalEvent<FaceHuggerComponent, BeingUnequippedAttemptEvent>(OnBeingUnequippedAttempt);

        // Goobstation - Throwing behavior
        SubscribeLocalEvent<ThrowableFacehuggerComponent, ThrownEvent>(OnThrown);
        SubscribeLocalEvent<ThrowableFacehuggerComponent, ThrowDoHitEvent>(OnThrowDoHit);
        // Dumont start
        SubscribeLocalEvent<FaceHuggerLeapComponent, ThrowDoHitEvent>(OnLeapHit);
        // Dumont end
    }

    private void OnCollideEvent(EntityUid uid, FaceHuggerComponent component, StartCollideEvent args)
    {
        // Dumont start
        if (!IsSentient(uid))
            TryEquipFaceHugger(uid, args.OtherEntity, component);
        // Dumont end
    }

    private void OnMeleeHit(EntityUid uid, FaceHuggerComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.FirstOrNull() is not { } target)
            return;

        TryEquipFaceHugger(uid, target, component);
    }

    private void OnPickedUp(EntityUid uid, FaceHuggerComponent component, GotEquippedHandEvent args)
    {
        // Dumont start
        if (!IsSentient(uid))
            TryEquipFaceHugger(uid, args.User, component);
        // Dumont end
    }

    private void OnStepTriggered(EntityUid uid, FaceHuggerComponent component, ref StepTriggeredOffEvent args)
    {
        if (component.Active && !IsSentient(uid)) // Dumont
            TryEquipFaceHugger(uid, args.Tripper, component);
    }

    private void OnGotEquipped(EntityUid uid, FaceHuggerComponent component, GotEquippedEvent args)
    {
        if (args.Slot != component.Slot
            || _mobState.IsDead(uid)
            || _entityWhitelist.IsBlacklistPass(component.Blacklist, args.Equipee))
            return;
        _popup.PopupEntity(Loc.GetString("xenomorphs-face-hugger-equip", ("equipment", uid)), uid, args.Equipee);
        _popup.PopupEntity(
            Loc.GetString("xenomorphs-face-hugger-equip-other",
                ("equipment", uid),
                ("target", Identity.Entity(args.Equipee, EntityManager))),
            uid,
            Filter.PvsExcept(args.Equipee),
            true);

        _stun.TryKnockdown(args.Equipee, component.KnockdownTime, true);

        if (component.InfectionPrototype.HasValue)
            EnsureComp<XenomorphPreventSuicideComponent>(args.Equipee); //Prevent suicide for infected

        if (!component.InfectionPrototype.HasValue)
            return;

        var protection = component.ProtectionRequired || HasHeadProtection(args.Equipee);
        component.ProtectionRequired = false;
        if (!protection)
        {
            // Inventory has to finish updating the clothing slot first.
            _readyToInfect.Enqueue(uid);
            return;
        }

        EnsureComp<DoAfterComponent>(uid);
        var doAfter = new DoAfterArgs(EntityManager, uid, component.ProtectionDelay,
            new FaceHuggerProtectionDoAfterEvent(), uid, target: args.Equipee, showTo: args.Equipee)
        {
            NeedHand = false,
            BreakOnMove = false,
            BreakOnDamage = false,
            RequireCanInteract = false,
            DistanceThreshold = null,
            MultiplyDelay = false,
            CancelDuplicate = false,
        };
        if (_doAfter.TryStartDoAfter(doAfter, out var id))
        {
            component.ProtectionDoAfter = id;
            component.NextStruggleSound = _timing.CurTime;
        }
        else
            _inventory.TryUnequip(args.Equipee, component.Slot, true, true);
    }

    private void OnGotUnequipped(EntityUid uid, FaceHuggerComponent component, GotUnequippedEvent args)
    {
        if (args.Slot != component.Slot)
            return;

        var id = component.ProtectionDoAfter;
        component.ProtectionDoAfter = null;
        component.NextInjectionTime = TimeSpan.Zero;
        _doAfter.Cancel(id);
        if (!HasComp<XenomorphInfectedComponent>(args.Equipee))
            RemComp<XenomorphPreventSuicideComponent>(args.Equipee);

        if (component.InfectionPrototype.HasValue && !TerminatingOrDeleted(uid) && !_mobState.IsDead(uid))
        {
            component.Active = false;
            _damageable.TryChangeDamage(uid, component.DamageOnInfect, true);
        }
    }

    private void OnProtectionFinished(EntityUid uid, FaceHuggerComponent component, FaceHuggerProtectionDoAfterEvent args)
    {
        if (component.ProtectionDoAfter != args.DoAfter.Id)
            return;

        component.ProtectionDoAfter = null;
        if (args.Cancelled || args.Handled || args.Target is not { } target || _mobState.IsDead(target))
            return;

        args.Handled = true;
        Infect(uid, component);
    }

    private void OnBeingUnequippedAttempt(EntityUid uid,
        FaceHuggerComponent component,
        BeingUnequippedAttemptEvent args)
    {
        if (component.Slot != args.Slot || args.Unequipee != args.UnEquipTarget ||
            !component.InfectionPrototype.HasValue || _mobState.IsDead(uid) || HasComp<FacehuggerImmuneComponent>(args.Unequipee)) // Omu, add check for FacehuggerImmune
            return;

        _popup.PopupEntity(
            Loc.GetString("xenomorphs-face-hugger-unequip", ("equipment", Identity.Entity(uid, EntityManager))),
            uid,
            args.Unequipee);
        args.Cancel();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        while (_readyToInfect.TryDequeue(out var ready))
        {
            if (TryComp<FaceHuggerComponent>(ready, out var component))
                Infect(ready, component);
        }

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<FaceHuggerComponent>();
        while (query.MoveNext(out var uid, out var faceHugger))
        {
            if (!faceHugger.Active && time > faceHugger.RestIn)
                faceHugger.Active = true;

            if (faceHugger.ProtectionDoAfter != null)
            {
                if (_mobState.IsDead(uid) || !_container.TryGetContainingContainer(uid, out var attached)
                    || _mobState.IsDead(attached.Owner))
                {
                    _doAfter.Cancel(faceHugger.ProtectionDoAfter);
                    continue;
                }

                if (time >= faceHugger.NextStruggleSound)
                {
                    faceHugger.NextStruggleSound = time + TimeSpan.FromSeconds(4);
                    var female = TryComp<HumanoidAppearanceComponent>(attached.Owner, out var appearance)
                        && appearance.Sex == Sex.Female;
                    _audio.PlayPvs(new SoundCollectionSpecifier(female ? "CMUFemaleFacehugged" : "CMUMaleFacehugged"), attached.Owner);
                    _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/acid_sizzle1.ogg"), attached.Owner);
                }
                continue;
            }

            // Handle continuous chemical injection when equipped
            // Goobstation
            if (TryComp<ClothingComponent>(uid, out var clothing) && clothing.InSlot != null && !_mobState.IsDead(uid))
            {
                // Initialize NextInjectionTime if it's zero
                if (faceHugger.NextInjectionTime == TimeSpan.Zero)
                {
                    faceHugger.NextInjectionTime = time + faceHugger.InitialInjectionDelay;
                    continue;
                }

                if (time >= faceHugger.NextInjectionTime)
                {
                    // Get the entity that has this item equipped
                    if (_container.TryGetContainingContainer(uid, out var container) && container.Owner != uid)
                    {
                        if (!HasComp<FacehuggerImmuneComponent>(container.Owner)) // Omu, don't inject into people who the facehugger wont infect
                        {
                            InjectChemicals(uid, faceHugger, container.Owner);
                        }
                        // Set the next injection time based on the current time plus interval
                        faceHugger.NextInjectionTime = time + faceHugger.InjectionInterval;
                    }
                }
            }
            // Goobstaion end

            if (faceHugger.Active && clothing?.InSlot == null && !IsSentient(uid))
                TryAutoLeap(uid, faceHugger);
        }
    }

    private void TryAutoLeap(EntityUid uid, FaceHuggerComponent component)
    {
        if (_mobState.IsDead(uid) || _container.IsEntityInContainer(uid) || HasComp<ThrownItemComponent>(uid)
            || !TryComp<FaceHuggerLeapComponent>(uid, out var leap) || leap.IsLeaping
            || TryComp<XenomorphCarrierRecoveryComponent>(uid, out var recovery) && _timing.CurTime < recovery.RecoverAt
            || _actions.GetAction(leap.LeapActionEntity) is not { } action || _actions.IsCooldownActive(action.Comp))
            return;

        foreach (var target in _entityLookup.GetEntitiesInRange<InventoryComponent>(Transform(uid).Coordinates, leap.AutoLeapRange))
        {
            if (target.Owner == uid || !HasComp<MobStateComponent>(target) || _mobState.IsDead(target)
                || HasComp<FacehuggerImmuneComponent>(target) || _entityWhitelist.IsBlacklistPass(component.Blacklist, target)
                || _container.IsEntityInContainer(target)
                || !_interaction.InRangeUnobstructed(uid, target.Owner, leap.AutoLeapRange))
                continue;

            if (_actions.TryPerformAction(uid, new RequestPerformActionEvent(GetNetEntity(action.Owner),
                    GetNetCoordinates(Transform(target).Coordinates))))
                break;
        }
    }

    // Dumont start
    private bool IsSentient(EntityUid uid) => TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind;

    private void OnLeapHit(Entity<FaceHuggerLeapComponent> ent, ref ThrowDoHitEvent args)
    {
        if (!ent.Comp.IsLeaping)
            return;

        ent.Comp.IsLeaping = false;
        Dirty(ent);
        if (HasComp<MobStateComponent>(args.Target) && TryComp<FaceHuggerComponent>(ent.Owner, out var faceHugger))
            TryEquipFaceHugger(ent.Owner, args.Target, faceHugger);
    }
    // Dumont end

    private void Infect(EntityUid uid, FaceHuggerComponent component)
    {
        if (!component.InfectionPrototype.HasValue
            || _mobState.IsDead(uid)
            || !TryComp<ClothingComponent>(uid, out var clothing)
            || clothing.InSlot != component.Slot
            || !_container.TryGetContainingContainer((uid, null, null), out var target))
            return;

        if (_mobState.IsDead(target.Owner) || HasComp<FacehuggerImmuneComponent>(target.Owner)
            || HasComp<XenomorphInfectedComponent>(target.Owner))
            return;

        var bodyPart = _body.GetBodyChildrenOfType(target.Owner,
                component.InfectionBodyPart.Type,
                symmetry: component.InfectionBodyPart.Symmetry)
            .FirstOrNull();
        if (!bodyPart.HasValue)
            return;

        var organ = Spawn(component.InfectionPrototype);
        _body.TryCreateOrganSlot(bodyPart.Value.Id, component.InfectionSlotId, out _, bodyPart.Value.Component);

        if (!_body.InsertOrgan(bodyPart.Value.Id, organ, component.InfectionSlotId, bodyPart.Value.Component))
        {
            QueueDel(organ);
            return;
        }

        // Dumont start
        if (_mind.TryGetMind(uid, out var mindId, out var mind)
            && TryComp<XenomorphInfectionComponent>(organ, out var infection))
        {
            infection.SourceMindId = mindId;
            _mind.TransferTo(mindId, organ, mind: mind);
        }
        // Dumont end

        var female = TryComp<HumanoidAppearanceComponent>(target.Owner, out var appearance) && appearance.Sex == Sex.Female;
        _audio.PlayPvs(new SoundCollectionSpecifier(female ? "CMUFemaleInfectionScream" : "CMUMaleInfectionScream"), target.Owner);
        _damageable.TryChangeDamage(uid, component.DamageOnInfect, true);
    }

    public bool TryEquipFaceHugger(EntityUid uid, EntityUid target, FaceHuggerComponent component)
    {
        if (_container.TryGetContainingContainer(uid, out var containing)
            && (HasComp<XenomorphResinHoleComponent>(containing.Owner) || HasComp<XenomorphTunnelComponent>(containing.Owner)))
            return false;

        if (TryComp<XenomorphCarrierRecoveryComponent>(uid, out var recovery) && _timing.CurTime < recovery.RecoverAt)
            return false;

        if (!component.Active || _mobState.IsDead(uid) || _mobState.IsDead(target) || _entityWhitelist.IsBlacklistPass(component.Blacklist, target))
            return false;

        if (HasComp<FacehuggerImmuneComponent>(target) || HasComp<XenomorphInfectedComponent>(target)
            || TryComp<ClothingComponent>(uid, out var worn) && worn.InSlot != null)
            return false;

        component.ProtectionRequired = HasHeadProtection(target)
            || _inventory.TryGetSlotEntity(target, component.Slot, out var mask)
                && TryComp<IngestionBlockerComponent>(mask, out var blocker) && blocker.Enabled
                && blocker.BlockSmokeIngestion;

        if (_inventory.TryGetSlotEntity(target, component.Slot, out _)
            && !_inventory.TryUnequip(target, component.Slot, true, true))
        {
            component.ProtectionRequired = false;
            return false;
        }

        // Set the rest time and deactivate
        var restTime = _random.Next(component.MinRestTime, component.MaxRestTime);
        component.RestIn = _timing.CurTime + restTime;
        component.Active = false;

        // Try to equip the facehugger
        var equipped = _inventory.TryEquip(target, uid, component.Slot, true, true);
        component.ProtectionRequired = false;
        return equipped;
    } // Gooobstation end

    #region Injection Code
    /// <summary>
    /// Checks if the facehugger can inject chemicals into the target
    /// Goobstation
    /// </summary>
    public bool CanInject(EntityUid uid, FaceHuggerComponent component, EntityUid target)
    {
        if (component.SleepChem == null) // Dumont
            return false;

        // Check if facehugger is properly equipped
        if (!TryComp<ClothingComponent>(uid, out var clothingComp) || clothingComp.InSlot == null)
        {
            if (!component.Active)
                return false;
            return true;
        }

        // Check if target already has the sleep chemical
        if (TryComp<BloodstreamComponent>(target, out var bloodstream) &&
            _solutions.ResolveSolution(target, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution, out var chemSolution) &&
            chemSolution.TryGetReagentQuantity(new ReagentId(component.SleepChem, null), out var quantity) &&
            quantity > FixedPoint2.New(component.MinChemicalThreshold))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Creates a solution with the sleep chemical
    /// </summary>
    public Solution CreateSleepChemicalSolution(FaceHuggerComponent component, float amount)
    {
        var solution = new Solution();
        if (component.SleepChem is { } reagent) // Dumont
            solution.AddReagent(reagent, amount);
        return solution;
    }

    /// <summary>
    /// Attempts to inject the solution into the target's bloodstream
    /// </summary>
    public bool TryInjectIntoBloodstream(EntityUid target, Solution solution, string chemName, float chemAmount)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return false;

        if (!_solutions.TryGetSolution(target, bloodstream.ChemicalSolutionName, out var chemSolution, out _))
            return false;

        if (!_solutions.TryAddSolution(chemSolution.Value, solution))
            return false;

        _reactiveSystem.DoEntityReaction(target, solution, ReactionMethod.Injection);
        return true;
    }

    /// <summary>
    /// Main method to handle chemical injection
    /// </summary>
    public void InjectChemicals(EntityUid uid, FaceHuggerComponent component, EntityUid target)
    {
        if (component.SleepChem is not { } reagent || !CanInject(uid, component, target))
            return;

        var sleepChem = CreateSleepChemicalSolution(component, component.SleepChemAmount);
        TryInjectIntoBloodstream(target, sleepChem, reagent, component.SleepChemAmount);
    }
    #endregion

    private bool HasHeadProtection(EntityUid target)
    {
        if (!_inventory.TryGetSlotEntity(target, "head", out var head))
            return false;

        if (TryComp<SealableClothingComponent>(head, out var sealable) && !sealable.IsSealed)
            return false;

        return HasComp<FaceHuggerBlockerComponent>(head);
    }

    #region Throwing Behavior

    /// <summary>
    /// Handles the start of a facehugger throw.
    /// Marks the facehugger as being in flight to track its state.
    /// Goobstation
    /// </summary>
    private void OnThrown(EntityUid uid, ThrowableFacehuggerComponent component, ThrownEvent args)
    {
        // Mark the facehugger as flying to track its airborne state
        component.IsFlying = true;

        // Make sure the facehugger is active when thrown
        if (TryComp<FaceHuggerComponent>(uid, out var faceHugger))
            faceHugger.Active = true;
    }

    /// <summary>
    /// Handles the facehugger's collision with a target after being thrown.
    /// Attempts to attach to a valid target if conditions are met.
    /// </summary>
    private void OnThrowDoHit(EntityUid uid, ThrowableFacehuggerComponent component, ThrowDoHitEvent args)
    {
        // Only process if the facehugger was actually thrown (not just dropped)
        if (!component.IsFlying)
            return;

        // Reset flying state as the throw has completed
        component.IsFlying = false;
        var target = args.Target;

        // Only proceed if the target is a valid living entity
        if (!HasComp<MobStateComponent>(target))
            return;

        // If this is a valid facehugger entity
        if (TryComp<FaceHuggerComponent>(uid, out var faceHugger))
            // Make sure the facehugger is active before trying to attach
            faceHugger.Active = true;
    }

    #endregion
}
