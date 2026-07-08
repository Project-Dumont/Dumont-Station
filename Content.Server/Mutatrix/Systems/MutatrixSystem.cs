// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Mutatrix.Components;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mutatrix.Components;
using Content.Shared.Mutatrix.Events;
using Content.Shared.Mutatrix.Prototypes;
using Content.Shared.Mutatrix.Systems;
using Content.Shared.Polymorph;
using Robust.Server.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Mutatrix.Systems;

/// <summary>
/// Server-side coordinator for the Mutatrix device.
///
/// Static/default forms still use the normal MutatrixTransformationPrototype list.
/// Dynamically scanned forms are stored as entity prototype IDs and are polymorphed
/// through PolymorphConfiguration directly, so the analyzer can support any mob or
/// species prototype without overwriting the 10 initial forms.
/// </summary>
public sealed class MutatrixSystem : SharedMutatrixSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    private static readonly HashSet<ProtoId<MutatrixTransformationPrototype>> RemovedBuiltIns = new()
    {
        new("MutatrixRat"),
        new("MutatrixIPC"),
        new("MutatrixPlasmaman"),
        new("MutatrixRevenant"),
        new("MutatrixChitinid"),
        new("MutatrixGhoulStalker"),
        new("MutatrixGosma"),
        new("MutatrixFeroxi"),
        new("MutatrixArachnid"),
        new("MutatrixBaseMobAsteroid"),
        new("MutatrixBesta"),
        new("MutatrixChama"),
        new("MutatrixQuatroBracos"),
        new("MutatrixGreyMatter"),
    };

    private static bool IsRemovedBuiltIn(ProtoId<MutatrixTransformationPrototype> transformation)
    {
        return RemovedBuiltIns.Contains(transformation);
    }

    private static readonly HashSet<string> DisallowedDynamicPrototypeIds = new()
    {
        // Mascotes/pets especiais continuam bloqueados para não duplicar personagens únicos.
        // MobMouse/MobMouse1/MobMouse2 foram removidos daqui: rato escaneado agora pode transformar.
        "MobTucanoBananilson",
        "MobWalter",
        "MobBingus",
        "MobCatRuntime",
        "MobCatException",
        "MobCatFloppa",
        "MobCorgiIan",
        "MobCorgiIanOld",
        "MobCorgiIanPup",
        "MobCorgiLisa",
        "MobCorgiMouse",
        "MobHamsterHamlet",
        "MobCatBingus",
    };


    private static bool IsBorgLikePrototypeId(string prototypeId)
    {
        var id = prototypeId.ToLowerInvariant();

        // Bloqueia borgs/cyborgs/xenoborgs/chassis. IPCs e sintéticos NÃO entram aqui.
        return id.Contains("borg")
            || id.Contains("cyborg")
            || id.Contains("xenoborg")
            || id.Contains("chassis");
    }

    private static bool ShouldTransferInventoryForDynamicPrototype(string prototypeId)
    {
        if (IsBorgLikePrototypeId(prototypeId))
            return false;

        var id = prototypeId.ToLowerInvariant();

        // Humanoides/sintéticos mantêm os itens equipados durante a transformação.
        return id.Contains("human")
            || id.Contains("reptilian")
            || id.Contains("lizard")
            || id.Contains("dwarf")
            || id.Contains("moth")
            || id.Contains("diona")
            || id.Contains("arachnid")
            || id.Contains("slimeperson")
            || id.Contains("slime")
            || id.Contains("vox")
            || id.Contains("kobold")
            || id.Contains("felinid")
            || id.Contains("oni")
            || id.Contains("harpy")
            || id.Contains("ipc")
            || id.Contains("synth")
            || id.Contains("synthetic")
            || id.Contains("android");
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MutatrixComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<MutatrixComponent, ClothingGotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<MutatrixComponent, ClothingGotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<MutatrixComponent, MutatrixOpenMenuActionEvent>(OnOpenMenuAction);

        SubscribeLocalEvent<MutatrixDnaComponent, PolymorphedEvent>(OnDnaPolymorphed);
        SubscribeLocalEvent<ActiveMutatrixComponent, PolymorphedEvent>(OnActivePolymorphed);

        Subs.BuiEvents<MutatrixComponent>(MutatrixUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnBoundUiOpened);
            subs.Event<MutatrixSelectTransformationMessage>(OnTransformationSelected);
            subs.Event<MutatrixSelectScannedPrototypeMessage>(OnScannedPrototypeSelected);
        });
    }

    private void OnGetItemActions(Entity<MutatrixComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.ActionEntity, ent.Comp.Action);
        args.AddAction(ref ent.Comp.CaptureActionEntity, ent.Comp.CaptureAction);
        Dirty(ent);
    }

    private void OnEquipped(Entity<MutatrixComponent> ent, ref ClothingGotEquippedEvent args)
    {
        EnsureWearerState(args.Wearer, ent.Owner);
    }

    private void OnUnequipped(Entity<MutatrixComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (!TryComp<ActiveMutatrixComponent>(args.Wearer, out var active))
            return;

        if (active.Device != ent.Owner)
            return;

        RemComp<ActiveMutatrixComponent>(args.Wearer);
    }

    private void OnOpenMenuAction(Entity<MutatrixComponent> ent, ref MutatrixOpenMenuActionEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;
        EnsureWearerState(user, ent.Owner);

        if (IsOnCooldown(user, ent.Comp, out var remaining))
        {
            ShowCooldown(user, remaining);
            args.Handled = true;
            return;
        }

        UpdateUserInterface(ent.Owner, ent.Comp, user);

        if (!_ui.TryOpenUi(ent.Owner, MutatrixUiKey.Key, user))
        {
            // Nunca transforma automaticamente se a UI falhar.
            // Antes isso escolhia a primeira forma desbloqueada, que fazia o jogador virar Laserraptor do nada.
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-menu-failed"), user, user);
        }

        args.Handled = true;
    }

    private void OnBoundUiOpened(Entity<MutatrixComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        EnsureWearerState(actor, ent.Owner);
        UpdateUserInterface(ent.Owner, ent.Comp, actor);
    }

    private void OnTransformationSelected(Entity<MutatrixComponent> ent, ref MutatrixSelectTransformationMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (!TryComp<MutatrixDnaComponent>(actor, out var dna))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-no-dna"), actor, actor);
            return;
        }

        EnsureDefaultUnlocks((actor, dna));

        if (IsOnCooldown(actor, ent.Comp, out var remaining))
        {
            ShowCooldown(actor, remaining);
            UpdateUserInterface(ent.Owner, ent.Comp, actor);
            return;
        }

        if (!IsUnlocked(dna, args.Transformation))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-locked"), actor, actor);
            UpdateUserInterface(ent.Owner, ent.Comp, actor);
            return;
        }

        dna.Selected = args.Transformation;
        dna.SelectedScannedPrototype = null;
        Dirty(actor, dna);

        _ui.CloseUi(ent.Owner, MutatrixUiKey.Key, actor);

        if (!TryTransform(actor, args.Transformation, dna))
            UpdateUserInterface(ent.Owner, ent.Comp, actor);
    }

    private void OnScannedPrototypeSelected(Entity<MutatrixComponent> ent, ref MutatrixSelectScannedPrototypeMessage args)
    {
        if (args.Actor is not { Valid: true } actor)
            return;

        if (!TryComp<MutatrixDnaComponent>(actor, out var dna))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-no-dna"), actor, actor);
            return;
        }

        EnsureDefaultUnlocks((actor, dna));

        if (IsOnCooldown(actor, ent.Comp, out var remaining))
        {
            ShowCooldown(actor, remaining);
            UpdateUserInterface(ent.Owner, ent.Comp, actor);
            return;
        }

        if (!IsDynamicUnlocked(dna, args.EntityPrototype))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-locked"), actor, actor);
            UpdateUserInterface(ent.Owner, ent.Comp, actor);
            return;
        }

        dna.Selected = null;
        dna.SelectedScannedPrototype = args.EntityPrototype;
        Dirty(actor, dna);

        _ui.CloseUi(ent.Owner, MutatrixUiKey.Key, actor);

        if (!TryTransformDynamic(actor, args.EntityPrototype, dna))
            UpdateUserInterface(ent.Owner, ent.Comp, actor);
    }

    private void OnDnaPolymorphed(Entity<MutatrixDnaComponent> ent, ref PolymorphedEvent args)
    {
        if (ent.Owner != args.OldEntity)
            return;

        if (Deleted(args.NewEntity))
            return;

        var targetDna = EnsureComp<MutatrixDnaComponent>(args.NewEntity);
        MergeDna(ent.Comp, targetDna);
        Dirty(args.NewEntity, targetDna);

        if (args.IsRevert)
        {
            var cooldown = EnsureComp<MutatrixCooldownComponent>(args.NewEntity);
            cooldown.EndTime = _timing.CurTime + TimeSpan.FromSeconds(GetRevertCooldown(args.NewEntity));
            Dirty(args.NewEntity, cooldown);
        }
    }

    private void OnActivePolymorphed(Entity<ActiveMutatrixComponent> ent, ref PolymorphedEvent args)
    {
        if (ent.Owner != args.OldEntity)
            return;

        if (Deleted(args.NewEntity))
            return;

        var targetActive = EnsureComp<ActiveMutatrixComponent>(args.NewEntity);
        targetActive.Device = ent.Comp.Device;
        Dirty(args.NewEntity, targetActive);
    }

    private void EnsureWearerState(EntityUid wearer, EntityUid device)
    {
        var active = EnsureComp<ActiveMutatrixComponent>(wearer);
        active.Device = device;
        Dirty(wearer, active);

        var dna = EnsureComp<MutatrixDnaComponent>(wearer);
        EnsureDefaultUnlocks((wearer, dna));
    }

    private void UpdateUserInterface(EntityUid device, MutatrixComponent component, EntityUid user)
    {
        var dna = EnsureComp<MutatrixDnaComponent>(user);
        EnsureDefaultUnlocks((user, dna));

        _ui.SetUiState(device,
            MutatrixUiKey.Key,
            new MutatrixBoundUserInterfaceState(
                GetAllUnlocked(dna),
                dna.Selected,
                GetAllDynamicUnlocked(dna),
                dna.SelectedScannedPrototype));
    }

    private bool TryGetSelectedOrFirstUnlocked(
        EntityUid user,
        out ProtoId<MutatrixTransformationPrototype>? transformation)
    {
        transformation = null;

        if (!TryComp<MutatrixDnaComponent>(user, out var dna))
            return false;

        EnsureDefaultUnlocks((user, dna));

        if (dna.Selected != null && IsUnlocked(dna, dna.Selected.Value))
        {
            transformation = dna.Selected.Value;
            return true;
        }

        foreach (var proto in _prototype.EnumeratePrototypes<MutatrixTransformationPrototype>()
                     .OrderBy(p => p.Order)
                     .ThenBy(p => p.ID))
        {
            if (!IsUnlocked(dna, proto.ID))
                continue;

            transformation = proto.ID;
            return true;
        }

        return false;
    }

    private bool TryTransform(
        EntityUid user,
        ProtoId<MutatrixTransformationPrototype> transformation,
        MutatrixDnaComponent? dna = null)
    {
        if (TryComp<PolymorphedEntityComponent>(user, out var polymorphed))
        {
            var original = _polymorph.Revert((user, polymorphed));
            if (original == null || Deleted(original.Value))
            {
                _popup.PopupEntity(Loc.GetString("mutatrix-popup-transform-failed"), user, user);
                return false;
            }

            user = original.Value;
            dna = EnsureComp<MutatrixDnaComponent>(user);
        }

        if (!Resolve(user, ref dna, false))
            return false;

        EnsureDefaultUnlocks((user, dna));

        if (!IsUnlocked(dna, transformation))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-locked"), user, user);
            return false;
        }

        if (TryComp<ActiveMutatrixComponent>(user, out var active)
            && TryComp<MutatrixComponent>(active.Device, out var device)
            && IsOnCooldown(user, device, out var remaining))
        {
            ShowCooldown(user, remaining);
            return false;
        }

        var originalName = Name(user);

        if (!_prototype.TryIndex(transformation, out var mutatrixTransformation))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-transform-failed"), user, user);
            return false;
        }

        if (!_prototype.TryIndex<PolymorphPrototype>(mutatrixTransformation.Polymorph, out _))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-transform-failed"), user, user);
            return false;
        }

        dna.Selected = transformation;
        dna.SelectedScannedPrototype = null;
        Dirty(user, dna);

        var result = _polymorph.PolymorphEntity(user, mutatrixTransformation.Polymorph);
        if (result == null)
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-transform-failed"), user, user);
            return false;
        }

        EnsureComp<MutatrixTransformedComponent>(result.Value);

        var childDna = EnsureComp<MutatrixDnaComponent>(result.Value);
        MergeDna(dna, childDna);
        childDna.Selected = transformation;
        childDna.SelectedScannedPrototype = null;
        Dirty(result.Value, childDna);

        if (TryComp<ActiveMutatrixComponent>(user, out active))
        {
            var childActive = EnsureComp<ActiveMutatrixComponent>(result.Value);
            childActive.Device = active.Device;
            Dirty(result.Value, childActive);
        }

        ApplyAppearanceOverrides(result.Value, mutatrixTransformation);
        ApplyOriginalName(result.Value, originalName);

        _popup.PopupEntity(
            Loc.GetString("mutatrix-popup-selected", ("name", Loc.GetString(mutatrixTransformation.Name))),
            result.Value,
            result.Value);

        return true;
    }

    private bool TryTransformDynamic(EntityUid user, string entityPrototypeId, MutatrixDnaComponent? dna = null)
    {
        if (string.IsNullOrWhiteSpace(entityPrototypeId) || DisallowedDynamicPrototypeIds.Contains(entityPrototypeId) || IsBorgLikePrototypeId(entityPrototypeId))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-transform-failed"), user, user);
            return false;
        }

        if (TryComp<PolymorphedEntityComponent>(user, out var polymorphed))
        {
            var original = _polymorph.Revert((user, polymorphed));
            if (original == null || Deleted(original.Value))
            {
                _popup.PopupEntity(Loc.GetString("mutatrix-popup-transform-failed"), user, user);
                return false;
            }

            user = original.Value;
            dna = EnsureComp<MutatrixDnaComponent>(user);
        }

        if (!Resolve(user, ref dna, false))
            return false;

        EnsureDefaultUnlocks((user, dna));

        if (!IsDynamicUnlocked(dna, entityPrototypeId))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-locked"), user, user);
            return false;
        }

        if (TryComp<ActiveMutatrixComponent>(user, out var active)
            && TryComp<MutatrixComponent>(active.Device, out var device)
            && IsOnCooldown(user, device, out var remaining))
        {
            ShowCooldown(user, remaining);
            return false;
        }

        var originalName = Name(user);

        var entProtoId = new EntProtoId(entityPrototypeId);
        if (!_prototype.TryIndex(entProtoId, out var entityPrototype))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-transform-failed"), user, user);
            return false;
        }

        dna.Selected = null;
        dna.SelectedScannedPrototype = entityPrototypeId;
        Dirty(user, dna);

        var config = CreateDynamicPolymorphConfiguration(entProtoId);
        var result = _polymorph.PolymorphEntity(user, config);
        if (result == null)
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-transform-failed"), user, user);
            return false;
        }

        EnsureComp<MutatrixTransformedComponent>(result.Value);

        var childDna = EnsureComp<MutatrixDnaComponent>(result.Value);
        MergeDna(dna, childDna);
        childDna.Selected = null;
        childDna.SelectedScannedPrototype = entityPrototypeId;
        Dirty(result.Value, childDna);

        if (TryComp<ActiveMutatrixComponent>(user, out active))
        {
            var childActive = EnsureComp<ActiveMutatrixComponent>(result.Value);
            childActive.Device = active.Device;
            Dirty(result.Value, childActive);
        }

        ApplyOriginalName(result.Value, originalName);

        var displayName = GetScannedDisplayName(entityPrototypeId, entityPrototype);

        _popup.PopupEntity(
            Loc.GetString("mutatrix-popup-selected", ("name", displayName)),
            result.Value,
            result.Value);

        return true;
    }


    private string GetScannedDisplayName(string prototypeId, EntityPrototype entityPrototype)
    {
        foreach (var species in _prototype.EnumeratePrototypes<SpeciesPrototype>())
        {
            if (species.Prototype.Id == prototypeId)
                return Loc.GetString(species.Name);
        }

        return string.IsNullOrWhiteSpace(entityPrototype.Name)
            ? prototypeId
            : Loc.GetString(entityPrototype.Name);
    }

    private static PolymorphConfiguration CreateDynamicPolymorphConfiguration(EntProtoId entityPrototype)
    {
        return new PolymorphConfiguration
        {
            Entity = entityPrototype,
            Duration = 360,
            Forced = false,
            TransferDamage = false,
            TransferName = true,
            TransferLanguageSpeaker = true,
            TransferSpeechBarks = false,
            TransferAccents = false,
            TransferQuirks = false,
            TransferHumanoidAppearance = false,
            Inventory = ShouldTransferInventoryForDynamicPrototype(entityPrototype.Id)
                ? PolymorphInventoryChange.Transfer
                : PolymorphInventoryChange.None,
            RevertOnCrit = false,
            RevertOnDeath = false,
            CanNotPolymorphInStorage = true,
            AllowRepeatedMorphs = false,
            IgnoreAllowRepeatedMorphs = false,
            Cooldown = TimeSpan.FromSeconds(90),
            SkipRevertConfirmation = true,
            ShowPopup = false,
            ComponentsToTransfer = new HashSet<ComponentTransferData>(),
        };
    }

    private void ApplyOriginalName(EntityUid uid, string originalName)
    {
        if (string.IsNullOrWhiteSpace(originalName))
            return;

        _metaData.SetEntityName(uid, originalName);
    }

    private void ApplyAppearanceOverrides(EntityUid uid, MutatrixTransformationPrototype transformation)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return;

        if (transformation.SkinColor != null)
            _humanoid.SetSkinColor(uid, transformation.SkinColor.Value, verify: false, humanoid: humanoid);

        if (transformation.EyeColor != null)
            humanoid.EyeColor = transformation.EyeColor.Value;

        if (transformation.ID == "MutatrixIPC")
        {
            ApplyIpcPalette(humanoid,
                transformation.SkinColor ?? Color.FromHex("#1A1A1A"),
                transformation.DetailColor ?? Color.FromHex("#39FF14"));
        }
        else if (transformation.ID == "MutatrixFeroxi")
        {
            ApplyFeroxiPalette(humanoid,
                transformation.SkinColor ?? Color.FromHex("#4F8F83"),
                transformation.DetailColor ?? Color.FromHex("#111417"));
        }

        Dirty(uid, humanoid);
    }

    private static void ApplyIpcPalette(HumanoidAppearanceComponent humanoid, Color body, Color detail)
    {
        humanoid.MarkingSet.EnsureDefault(body, detail);

        foreach (var markings in humanoid.MarkingSet.Markings.Values)
        {
            foreach (var marking in markings)
                marking.SetColor(body);
        }

        ReplaceForcedMarking(humanoid.MarkingSet, MarkingCategories.Face, "ScreenRing", detail);
    }

    private static void ApplyFeroxiPalette(HumanoidAppearanceComponent humanoid, Color body, Color detail)
    {
        var countershade = Color.FromHex("#D7E8C8");

        humanoid.MarkingSet.EnsureDefault(body, detail);

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.HeadTop,
            CreateForcedMarking("FeroxiEarsAltTips", body, countershade, detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.Snout,
            CreateForcedMarking("FeroxiSnoutCountershadingStripe", body, countershade, detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.Tail,
            CreateForcedMarking("FeroxiFullTipTwoToneTailAndDorsal",
                body,
                detail,
                detail,
                countershade,
                body,
                detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.Head,
            CreateForcedMarking("FeroxiHeadStripesTigerAlt", detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.Chest,
            CreateForcedMarking("FeroxiTorsoCountershadingM", countershade),
            CreateForcedMarking("FeroxiTorsoStripesTigerAlt", detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.Arms,
            CreateForcedMarking("FeroxiLArmStripesTiger", detail),
            CreateForcedMarking("FeroxiRArmStripesTiger", detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.Legs,
            CreateForcedMarking("FeroxiLLegStripesTiger", detail),
            CreateForcedMarking("FeroxiRLegStripesTiger", detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.LeftHand,
            CreateForcedMarking("FeroxiLHandTip", detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.RightHand,
            CreateForcedMarking("FeroxiRHandTip", detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.LeftFoot,
            CreateForcedMarking("FeroxiLFootTip", detail));

        ReplaceCategoryWithForcedMarkings(humanoid.MarkingSet, MarkingCategories.RightFoot,
            CreateForcedMarking("FeroxiRFootTip", detail));
    }

    private static void ReplaceForcedMarking(MarkingSet set, MarkingCategories category, string id, params Color[] colors)
    {
        ReplaceCategoryWithForcedMarkings(set, category, CreateForcedMarking(id, colors));
    }

    private static void ReplaceCategoryWithForcedMarkings(MarkingSet set, MarkingCategories category, params Marking[] markings)
    {
        set.RemoveCategory(category);

        foreach (var marking in markings)
            set.AddBack(category, marking);
    }

    private static Marking CreateForcedMarking(string id, params Color[] colors)
    {
        return new Marking(id, colors.ToList())
        {
            Forced = true,
        };
    }

    private bool IsOnCooldown(EntityUid user, MutatrixComponent device, out TimeSpan remaining)
    {
        remaining = TimeSpan.Zero;

        if (!TryComp<MutatrixCooldownComponent>(user, out var cooldown))
            return false;

        if (_timing.CurTime >= cooldown.EndTime)
        {
            RemCompDeferred<MutatrixCooldownComponent>(user);
            return false;
        }

        remaining = cooldown.EndTime - _timing.CurTime;
        return true;
    }

    private float GetRevertCooldown(EntityUid user)
    {
        if (TryComp<ActiveMutatrixComponent>(user, out var active)
            && TryComp<MutatrixComponent>(active.Device, out var device))
            return device.RevertCooldown;

        return 90f;
    }

    private void ShowCooldown(EntityUid user, TimeSpan remaining)
    {
        _popup.PopupEntity(
            Loc.GetString("mutatrix-popup-cooldown", ("seconds", (int) Math.Ceiling(remaining.TotalSeconds))),
            user,
            user);
    }

    private static void MergeDna(MutatrixDnaComponent source, MutatrixDnaComponent target)
    {
        foreach (var unlocked in source.Unlocked)
        {
            if (IsRemovedBuiltIn(unlocked))
                continue;

            target.Unlocked.Add(unlocked);
        }

        foreach (var roundUnlocked in source.RoundUnlocked)
        {
            if (IsRemovedBuiltIn(roundUnlocked))
                continue;

            target.RoundUnlocked.Add(roundUnlocked);
        }

        foreach (var dynamicUnlocked in source.RoundScannedPrototypes)
        {
            if (DisallowedDynamicPrototypeIds.Contains(dynamicUnlocked) || IsBorgLikePrototypeId(dynamicUnlocked))
                continue;

            target.RoundScannedPrototypes.Add(dynamicUnlocked);
        }

        target.Selected = source.Selected != null && IsRemovedBuiltIn(source.Selected.Value)
            ? null
            : source.Selected;

        target.SelectedScannedPrototype = source.SelectedScannedPrototype != null
                                      && (DisallowedDynamicPrototypeIds.Contains(source.SelectedScannedPrototype) || IsBorgLikePrototypeId(source.SelectedScannedPrototype))
            ? null
            : source.SelectedScannedPrototype;
    }
}
