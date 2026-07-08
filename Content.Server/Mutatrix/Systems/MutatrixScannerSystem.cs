// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mobs.Components;
using Content.Shared.Mutatrix.Components;
using Content.Shared.Mutatrix.Events;
using Content.Shared.Mutatrix.Prototypes;
using Content.Shared.Mutatrix.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Mutatrix.Systems;

/// <summary>
/// Dynamic species scanner for the Mutatrix.
///
/// The scanner now stores the entity prototype of the scanned mob/species instead
/// of trying to map everything to a fixed YAML list. This keeps the 10 initial
/// forms unchanged and allows the analyzer to unlock any real mob/species from
/// the code for the rest of the round, except blocked pets/mascots.
/// </summary>
public sealed class MutatrixScannerSystem : SharedMutatrixSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private TimeSpan _nextScanUpdate;

    private static readonly HashSet<string> DisallowedScanPrototypeIds = new()
    {
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

    private static readonly HashSet<string> DisallowedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Bananilson",
        "Walter",
        "Ian",
        "Old Ian",
        "Puppy Ian",
        "Lisa",
        "Aves",
        "Runtime",
        "Exception",
        "Bingus",
    };


    private static bool IsBorgLikePrototypeId(string prototypeId)
    {
        var id = prototypeId.ToLowerInvariant();

        // Bloqueia borgs/cyborgs/xenoborgs/chassis. IPCs e sintéticos continuam permitidos.
        return id.Contains("borg")
            || id.Contains("cyborg")
            || id.Contains("xenoborg")
            || id.Contains("chassis");
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActiveMutatrixComponent, ComponentShutdown>(OnActiveShutdown);
        SubscribeLocalEvent<ActiveMutatrixComponent, MutatrixScanDoAfterEvent>(OnScanDoAfter);
        SubscribeLocalEvent<ActiveMutatrixComponent, DoAfterAttemptEvent<MutatrixScanDoAfterEvent>>(OnScanAttempt);
        SubscribeLocalEvent<MutatrixComponent, MutatrixCaptureDnaActionEvent>(OnCaptureDnaAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateScanProgress();

        var now = _timing.CurTime;
        if (now < _nextScanUpdate)
            return;

        _nextScanUpdate = now + TimeSpan.FromSeconds(1);

        var query = EntityQueryEnumerator<ActiveMutatrixComponent, MutatrixDnaComponent>();
        while (query.MoveNext(out var user, out var active, out var dna))
        {
            TryStartAutomaticScan(user, active, dna);
        }
    }

    private void OnCaptureDnaAction(Entity<MutatrixComponent> ent, ref MutatrixCaptureDnaActionEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;

        if (!TryComp<ActiveMutatrixComponent>(user, out var active) || active.Device != ent.Owner)
        {
            args.Handled = true;
            return;
        }

        if (TryComp<MutatrixCooldownComponent>(user, out var cooldown) && cooldown.EndTime > _timing.CurTime)
        {
            _popup.PopupEntity(
                Loc.GetString("mutatrix-popup-cooldown", ("seconds", (int) Math.Ceiling((cooldown.EndTime - _timing.CurTime).TotalSeconds))),
                user,
                user);
            args.Handled = true;
            return;
        }

        if (HasComp<MutatrixScanComponent>(user))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-scan-already-running"), user, user);
            args.Handled = true;
            return;
        }

        var target = args.Target;
        if (target == user || Deleted(target))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-no-scan-target"), user, user);
            args.Handled = true;
            return;
        }

        if (!_transform.InRange(Transform(user).Coordinates, Transform(target).Coordinates, ent.Comp.ScanRange + 0.05f))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-scan-too-far"), user, user);
            args.Handled = true;
            return;
        }

        if (!HasComp<MobStateComponent>(target) && !HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-no-scan-target"), user, user);
            args.Handled = true;
            return;
        }

        var dna = EnsureComp<MutatrixDnaComponent>(user);
        EnsureDefaultUnlocks((user, dna));

        if (!TryGetScannablePrototype(target, dna, out var scannedPrototype, out var displayName, out var alreadyUnlocked))
        {
            _popup.PopupEntity(Loc.GetString("mutatrix-popup-no-scan-target"), user, user);
            args.Handled = true;
            return;
        }

        if (alreadyUnlocked)
        {
            _popup.PopupEntity(
                Loc.GetString("mutatrix-popup-scan-known", ("name", displayName)),
                user,
                user);
            args.Handled = true;
            return;
        }

        StartScan(user, ent.Owner, target, scannedPrototype, ent.Comp);
        args.Handled = true;
    }

    private void OnActiveShutdown(Entity<ActiveMutatrixComponent> ent, ref ComponentShutdown args)
    {
        RemCompDeferred<MutatrixScanComponent>(ent.Owner);
    }

    private void OnScanAttempt(Entity<ActiveMutatrixComponent> ent, ref DoAfterAttemptEvent<MutatrixScanDoAfterEvent> args)
    {
        if (!TryComp<MutatrixComponent>(ent.Comp.Device, out var device))
        {
            args.Cancel();
            return;
        }

        if (TryComp<MutatrixCooldownComponent>(ent.Owner, out var cooldown) && cooldown.EndTime > _timing.CurTime)
        {
            args.Cancel();
            return;
        }

        if (!EntityManager.TryGetEntity(args.Event.TargetEntity, out var target) || target == null)
        {
            args.Cancel();
            return;
        }

        if (Deleted(target.Value) || Deleted(ent.Owner))
        {
            args.Cancel();
            return;
        }

        if (!_transform.InRange(Transform(ent.Owner).Coordinates, Transform(target.Value).Coordinates, device.ScanRange + 0.05f))
        {
            args.Cancel();
            return;
        }

        if (!TryComp<MutatrixDnaComponent>(ent.Owner, out var dna))
        {
            args.Cancel();
            return;
        }

        if (IsDynamicUnlocked(dna, args.Event.ScannedPrototype))
            args.Cancel();
    }

    private void OnScanDoAfter(Entity<ActiveMutatrixComponent> ent, ref MutatrixScanDoAfterEvent args)
    {
        if (args.Handled)
            return;

        var hadScan = TryComp<MutatrixScanComponent>(ent.Owner, out var scan)
            && scan.ScannedPrototype == args.ScannedPrototype;

        if (hadScan)
            RemCompDeferred<MutatrixScanComponent>(ent.Owner);

        if (args.Cancelled)
        {
            if (hadScan)
                _popup.PopupEntity(Loc.GetString("mutatrix-popup-scan-cancelled"), ent.Owner, ent.Owner);

            args.Handled = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(args.ScannedPrototype))
        {
            args.Handled = true;
            return;
        }

        if (!TryComp<MutatrixDnaComponent>(ent.Owner, out var dna))
        {
            args.Handled = true;
            return;
        }

        if (IsDynamicUnlocked(dna, args.ScannedPrototype))
        {
            args.Handled = true;
            return;
        }

        var entityProtoId = new EntProtoId(args.ScannedPrototype);
        if (!_prototype.TryIndex(entityProtoId, out var entityPrototype))
        {
            args.Handled = true;
            return;
        }

        dna.RoundScannedPrototypes.Add(args.ScannedPrototype);
        Dirty(ent.Owner, dna);

        var displayName = GetScannedDisplayName(args.ScannedPrototype, entityPrototype);

        _popup.PopupEntity(
            Loc.GetString("mutatrix-popup-scan-complete", ("name", displayName)),
            ent.Owner,
            ent.Owner);

        args.Handled = true;
    }

    private void UpdateScanProgress()
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<MutatrixScanComponent>();
        while (query.MoveNext(out var user, out var scan))
        {
            if (scan.Target == null || string.IsNullOrWhiteSpace(scan.ScannedPrototype))
            {
                RemCompDeferred<MutatrixScanComponent>(user);
                continue;
            }

            var total = scan.EndTime - scan.StartTime;
            if (total <= TimeSpan.Zero)
            {
                scan.Progress = 1f;
                Dirty(user, scan);
                continue;
            }

            var elapsed = now - scan.StartTime;
            var progress = Math.Clamp((float) (elapsed.TotalSeconds / total.TotalSeconds), 0f, 1f);

            if (Math.Abs(scan.Progress - progress) < 0.02f)
                continue;

            scan.Progress = progress;
            Dirty(user, scan);
        }
    }

    private void TryStartAutomaticScan(EntityUid user, ActiveMutatrixComponent active, MutatrixDnaComponent dna)
    {
        if (HasComp<MutatrixScanComponent>(user))
            return;

        if (!TryComp<MutatrixComponent>(active.Device, out var device) || !device.AutoScan)
            return;

        EnsureDefaultUnlocks((user, dna));

        if (TryFindUnknownNearbyTarget(user, device, dna, out var target, out var scannedPrototype))
            StartScan(user, active.Device, target, scannedPrototype, device);
    }

    private bool TryFindUnknownNearbyTarget(
        EntityUid user,
        MutatrixComponent device,
        MutatrixDnaComponent dna,
        out EntityUid target,
        out string scannedPrototype)
    {
        target = default;
        scannedPrototype = string.Empty;

        var coordinates = Transform(user).Coordinates;
        foreach (var (candidate, _) in _lookup.GetEntitiesInRange<MobStateComponent>(coordinates, device.ScanRange, LookupFlags.Dynamic | LookupFlags.Approximate))
        {
            if (candidate == user || Deleted(candidate))
                continue;

            if (!TryGetScannablePrototype(candidate, dna, out var candidatePrototype, out _, out var alreadyUnlocked))
                continue;

            if (alreadyUnlocked)
                continue;

            target = candidate;
            scannedPrototype = candidatePrototype;
            return true;
        }

        return false;
    }

    private bool TryGetScannablePrototype(
        EntityUid target,
        MutatrixDnaComponent dna,
        out string scannedPrototype,
        out string displayName,
        out bool alreadyUnlocked)
    {
        scannedPrototype = string.Empty;
        displayName = string.Empty;
        alreadyUnlocked = false;

        if (MetaData(target).EntityPrototype is not { } entityPrototype)
            return false;

        if (IsDisallowedScanTarget(target, entityPrototype))
            return false;

        // If the target is already represented by one of the 10 built-in forms,
        // do not add a duplicate dynamic button. This keeps the initial list clean.
        if (TryGetBuiltInTransformationForTarget(target, entityPrototype, out var builtIn))
        {
            if (IsUnlocked(dna, builtIn.ID))
            {
                displayName = Loc.GetString(builtIn.Name);
                alreadyUnlocked = true;
                return true;
            }
        }

        EntProtoId dynamicPrototype;

        // Humanoid races are stored as their SpeciesPrototype.Prototype, not as
        // the current named character entity. This gives generic Human, Lizard,
        // Kobold, etc. instead of copying the exact person.
        if (TryComp<HumanoidAppearanceComponent>(target, out var humanoid)
            && _prototype.TryIndex<SpeciesPrototype>(humanoid.Species, out var species)
            && _prototype.TryIndex(species.Prototype, out _))
        {
            dynamicPrototype = species.Prototype;
            displayName = Loc.GetString(species.Name);
        }
        else
        {
            dynamicPrototype = new EntProtoId(entityPrototype.ID);
            displayName = string.IsNullOrWhiteSpace(entityPrototype.Name)
                ? entityPrototype.ID
                : Loc.GetString(entityPrototype.Name);
        }

        scannedPrototype = dynamicPrototype.Id;

        if (DisallowedScanPrototypeIds.Contains(scannedPrototype) || IsBorgLikePrototypeId(scannedPrototype))
            return false;

        alreadyUnlocked = IsDynamicUnlocked(dna, scannedPrototype);
        return true;
    }

    private bool TryGetBuiltInTransformationForTarget(
        EntityUid target,
        EntityPrototype targetPrototype,
        out MutatrixTransformationPrototype transformation)
    {
        transformation = default!;

        foreach (var proto in _prototype.EnumeratePrototypes<MutatrixTransformationPrototype>()
                     .Where(proto => proto.CanScan)
                     .OrderBy(proto => proto.Order)
                     .ThenBy(proto => proto.ID))
        {
            if (!MatchesTarget(target, targetPrototype, proto))
                continue;

            transformation = proto;
            return true;
        }

        return false;
    }

    private bool MatchesTarget(EntityUid target, EntityPrototype targetPrototype, MutatrixTransformationPrototype transformation)
    {
        if (MatchesEntityPrototype(targetPrototype, transformation))
            return true;

        if (TryComp<HumanoidAppearanceComponent>(target, out var humanoid)
            && transformation.ScanSpecies.Contains(humanoid.Species))
            return true;

        return false;
    }

    private bool MatchesEntityPrototype(EntityPrototype targetPrototype, MutatrixTransformationPrototype transformation)
    {
        if (MatchesScanSource(targetPrototype, transformation.MobPrototype))
            return true;

        foreach (var source in transformation.ScanPrototypes)
        {
            if (MatchesScanSource(targetPrototype, source))
                return true;
        }

        return false;
    }

    private bool MatchesScanSource(EntityPrototype targetPrototype, EntProtoId source)
    {
        if (targetPrototype.ID == source.Id)
            return true;

        foreach (var parent in _prototype.EnumerateParents<EntityPrototype>(targetPrototype.ID))
        {
            if (parent.ID == source.Id)
                return true;
        }

        return false;
    }

    private bool IsDisallowedScanTarget(EntityUid target, EntityPrototype targetPrototype)
    {
        if (DisallowedScanPrototypeIds.Contains(targetPrototype.ID) || IsBorgLikePrototypeId(targetPrototype.ID))
            return true;

        if (DisallowedNames.Contains(Name(target)))
            return true;

        return false;
    }

    private void StartScan(
        EntityUid user,
        EntityUid deviceUid,
        EntityUid target,
        string scannedPrototype,
        MutatrixComponent device)
    {
        var scanTime = device.ScanTime;
        var start = _timing.CurTime;
        var end = start + TimeSpan.FromSeconds(scanTime);

        var scan = EnsureComp<MutatrixScanComponent>(user);
        scan.Target = target;
        scan.ScannedPrototype = scannedPrototype;
        scan.StartTime = start;
        scan.EndTime = end;
        scan.Progress = 0f;
        Dirty(user, scan);

        var doAfter = new DoAfterArgs(
            EntityManager,
            user,
            TimeSpan.FromSeconds(scanTime),
            new MutatrixScanDoAfterEvent(GetNetEntity(target), scannedPrototype),
            user,
            target: target,
            used: deviceUid)
        {
            NeedHand = false,
            RequireCanInteract = false,
            BreakOnMove = false,
            BreakOnDamage = false,
            DistanceThreshold = device.ScanRange + 0.05f,
            CancelDuplicate = true,
            BlockDuplicate = true,
            MultiplyDelay = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            RemCompDeferred<MutatrixScanComponent>(user);
            return;
        }

        var startDisplayName = Name(target);
        var entProtoId = new EntProtoId(scannedPrototype);
        if (_prototype.TryIndex(entProtoId, out var entityPrototype))
            startDisplayName = GetScannedDisplayName(scannedPrototype, entityPrototype);

        _popup.PopupEntity(
            Loc.GetString("mutatrix-popup-scan-start", ("target", startDisplayName)),
            user,
            user);
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
}
