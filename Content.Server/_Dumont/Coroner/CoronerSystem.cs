using System.Linq;
using System.Text;
using Content.Shared._Dumont.Coroner;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Forensics.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Paper;
using Content.Server.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Coroner;

public sealed class CoronerSystem : SharedCoronerSystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MobStateSystem _mobStates = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly TraumaSystem _traumas = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<DamageHistoryComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        EnsureComp<TimeOfDeathComponent>(args.Target, out var timeOfDeath);
        timeOfDeath.Time = _timing.CurTime;
    }

    private void OnDamageChanged(Entity<DamageHistoryComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null || _mobStates.IsDead(ent))
            return;

        var worst = args.DamageDelta.DamageDict
            .Where(damage => damage.Value > 0)
            .OrderByDescending(damage => damage.Value)
            .FirstOrDefault();

        if (worst.Key == null)
            return;

        var now = _timing.CurTime;

        for (var i = ent.Comp.Records.Count - 1; i >= 0; i--)
        {
            var record = ent.Comp.Records[i];
            if (record.Type != worst.Key || now - record.LastTime > ent.Comp.MergeWindow)
                continue;

            record.Amount += worst.Value;
            record.LastTime = now;
            return;
        }

        ent.Comp.Records.Add(new DamageRecord
        {
            Time = now,
            LastTime = now,
            Type = worst.Key,
            Amount = worst.Value,
        });

        if (ent.Comp.Records.Count > ent.Comp.MaxRecords)
            ent.Comp.Records.RemoveAt(0);
    }

    protected override void Autopsy(Entity<AutopsyToolComponent> ent, EntityUid user, EntityUid target)
    {
        var report = Spawn(ent.Comp.Report, Transform(user).Coordinates);

        if (TryComp<PaperComponent>(report, out var paper))
            _paper.SetContent((report, paper), GetReport(target));

        _metaData.SetEntityName(report, Loc.GetString("autopsy-report-name",
            ("target", Identity.Entity(target, EntityManager))));

        _hands.PickupOrDrop(user, report);
    }

    private string GetReport(EntityUid target)
    {
        var report = new StringBuilder();
        report.Append(Loc.GetString("autopsy-report-identity",
            ("target", Identity.Entity(target, EntityManager)),
            ("species", GetSpecies(target)),
            ("sex", GetSex(target))));
        report.Append('\n');
        report.Append(GetOwnDna(target));
        report.Append('\n');
        report.Append(GetTimeOfDeath(target));
        report.Append('\n');
        report.Append(GetDamage(target));
        report.Append('\n');
        report.Append(GetWounds(target));
        report.Append('\n');
        report.Append(GetTraumas(target));
        report.Append('\n');
        report.Append(GetHistory(target));
        report.Append('\n');
        report.Append(GetChemicals(target));
        report.Append('\n');
        report.Append(GetForensics(target));

        return report.ToString();
    }

    private string GetSpecies(EntityUid target)
    {
        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoid)
            || !_prototype.TryIndex<SpeciesPrototype>(humanoid.Species, out var species))
            return Loc.GetString("autopsy-unknown");

        return Loc.GetString(species.Name);
    }

    private string GetSex(EntityUid target)
    {
        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoid))
            return Loc.GetString("autopsy-unknown");

        return Loc.GetString($"autopsy-sex-{humanoid.Sex.ToString().ToLowerInvariant()}");
    }

    private string GetOwnDna(EntityUid target)
    {
        if (!TryComp<DnaComponent>(target, out var dna) || dna.DNA == null)
            return Loc.GetString("autopsy-report-own-dna-unknown");

        return Loc.GetString("autopsy-report-own-dna", ("dna", dna.DNA));
    }

    private string GetTimeOfDeath(EntityUid target)
    {
        if (!TryComp<TimeOfDeathComponent>(target, out var timeOfDeath))
            return Loc.GetString("autopsy-report-time-unknown");

        var time = timeOfDeath.Time - _gameTicker.RoundStartTimeSpan;

        return Loc.GetString("autopsy-report-time", ("time", time.ToString("hh\\:mm\\:ss")));
    }

    private string GetDamage(EntityUid target)
    {
        if (!TryComp<DamageableComponent>(target, out var damageable))
            return Loc.GetString("autopsy-report-damage-none");

        var groups = damageable.DamagePerGroup
            .Where(group => group.Value.Int() > 0)
            .OrderByDescending(group => group.Value)
            .ToList();

        if (groups.Count == 0)
            return Loc.GetString("autopsy-report-damage-none");

        var damage = new StringBuilder();
        damage.Append(Loc.GetString("autopsy-report-cause", ("cause", GetGroupName(groups[0].Key))));
        damage.Append('\n');
        damage.Append(Loc.GetString("autopsy-report-damage-header"));

        foreach (var (group, amount) in groups)
        {
            damage.Append('\n');
            damage.Append(Loc.GetString("autopsy-report-damage",
                ("group", GetGroupName(group)),
                ("amount", amount.Int())));
        }

        return damage.ToString();
    }

    private string GetGroupName(string group)
    {
        return _prototype.TryIndex<DamageGroupPrototype>(group, out var proto)
            ? proto.LocalizedName
            : group;
    }

    private string GetWounds(EntityUid target)
    {
        if (!_wounds.TryGetAllOwnerWounds(target, out var wounds))
            return Loc.GetString("autopsy-report-wounds-none");

        var report = new StringBuilder();
        report.Append(Loc.GetString("autopsy-report-wounds-header"));

        foreach (var wound in wounds
                     .Where(wound => !wound.Comp.IsScar && wound.Comp.WoundSeverity != WoundSeverity.Healed)
                     .OrderByDescending(wound => wound.Comp.WoundSeverityPoint))
        {
            report.Append('\n');
            report.Append(Loc.GetString("autopsy-report-wound",
                ("part", GetPartName(wound.Comp.HoldingWoundable)),
                ("severity", GetSeverityName(wound.Comp.WoundSeverity)),
                ("wound", GetWoundName(wound))));
        }

        return report.ToString();
    }

    private string GetHistory(EntityUid target)
    {
        if (!TryComp<DamageHistoryComponent>(target, out var history))
            return Loc.GetString("autopsy-report-history-none");

        var records = history.Records.Where(record => record.Amount >= history.MinDamage).ToList();
        if (records.Count == 0)
            return Loc.GetString("autopsy-report-history-none");

        var report = new StringBuilder();
        report.Append(Loc.GetString("autopsy-report-history-header"));

        foreach (var record in records)
        {
            report.Append('\n');
            report.Append(Loc.GetString("autopsy-report-history",
                ("time", (record.Time - _gameTicker.RoundStartTimeSpan).ToString("hh\\:mm\\:ss")),
                ("type", GetTypeName(record.Type)),
                ("amount", record.Amount.Int())));
        }

        return report.ToString();
    }

    private string GetTypeName(string type)
    {
        return _prototype.TryIndex<DamageTypePrototype>(type, out var proto)
            ? proto.LocalizedName
            : type;
    }

    private string GetTraumas(EntityUid target)
    {
        if (!_traumas.TryGetBodyTraumas(target, out var traumas))
            return Loc.GetString("autopsy-report-traumas-none");

        var report = new StringBuilder();
        report.Append(Loc.GetString("autopsy-report-traumas-header"));

        foreach (var trauma in traumas
                     .Where(trauma => !IsHealthyBone(trauma))
                     .OrderByDescending(trauma => trauma.Comp.TraumaSeverity))
        {
            report.Append('\n');
            report.Append(Loc.GetString("autopsy-report-trauma",
                ("part", GetTraumaPartName(trauma)),
                ("trauma", GetTraumaName(trauma))));
        }

        return report.ToString();
    }

    private bool IsHealthyBone(Entity<TraumaComponent> trauma)
    {
        return trauma.Comp.TraumaType == TraumaType.BoneDamage
               && trauma.Comp.TraumaTarget is { } bone
               && TryComp<BoneComponent>(bone, out var boneComp)
               && boneComp.BoneSeverity == BoneSeverity.Normal;
    }

    private string GetTraumaPartName(Entity<TraumaComponent> trauma)
    {
        if (trauma.Comp.TraumaType == TraumaType.Dismemberment && trauma.Comp.TargetType is { } targetType)
            return GetPartName(targetType.Item1, targetType.Item2);

        return trauma.Comp.HoldingWoundable is { } woundable
            ? GetPartName(woundable)
            : Loc.GetString("autopsy-part-other");
    }

    private string GetTraumaName(Entity<TraumaComponent> trauma)
    {
        switch (trauma.Comp.TraumaType)
        {
            case TraumaType.BoneDamage:
                var severity = trauma.Comp.TraumaTarget is { } bone && TryComp<BoneComponent>(bone, out var boneComp)
                    ? boneComp.BoneSeverity
                    : BoneSeverity.Damaged;

                return Loc.GetString($"autopsy-bone-{severity.ToString().ToLowerInvariant()}");

            case TraumaType.OrganDamage:
                if (trauma.Comp.TraumaTarget is not { } organ)
                    return Loc.GetString("autopsy-trauma-organ-unknown");

                if (TryComp<OrganComponent>(organ, out var organComp)
                    && Loc.TryGetString($"autopsy-organ-{organComp.SlotId.Replace('_', '-')}", out var organName))
                    return organName;

                return Loc.GetString("autopsy-trauma-organ", ("organ", Name(organ)));

            default:
                return Loc.GetString($"autopsy-trauma-{trauma.Comp.TraumaType.ToString().ToLowerInvariant()}");
        }
    }

    private string GetChemicals(EntityUid target)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream)
            || !_solutionContainer.TryGetSolution(target, bloodstream.ChemicalSolutionName, out _, out var solution)
            || solution.Contents.Count == 0)
            return Loc.GetString("autopsy-report-chemicals-none");

        var chemicals = new StringBuilder();
        chemicals.Append(Loc.GetString("autopsy-report-chemicals-header"));

        foreach (var reagent in solution.Contents.OrderByDescending(reagent => reagent.Quantity))
        {
            var name = _prototype.TryIndex<ReagentPrototype>(reagent.Reagent.Prototype, out var proto)
                ? proto.LocalizedName
                : reagent.Reagent.Prototype;

            chemicals.Append('\n');
            chemicals.Append(Loc.GetString("autopsy-report-chemical",
                ("reagent", name),
                ("amount", reagent.Quantity.Int())));
        }

        return chemicals.ToString();
    }

    private string GetForensics(EntityUid target)
    {
        if (!TryComp<ForensicsComponent>(target, out var forensics))
            return Loc.GetString("autopsy-report-forensics-none");

        var clues = new StringBuilder();
        clues.Append(Loc.GetString("autopsy-report-forensics-header"));

        var found = false;

        foreach (var fingerprint in forensics.Fingerprints)
        {
            clues.Append('\n');
            clues.Append(Loc.GetString("autopsy-report-fingerprint", ("print", fingerprint)));
            found = true;
        }

        foreach (var (dna, _) in forensics.DNAs)
        {
            clues.Append('\n');
            clues.Append(Loc.GetString("autopsy-report-dna", ("dna", dna)));
            found = true;
        }

        foreach (var fiber in forensics.Fibers)
        {
            clues.Append('\n');
            clues.Append(Loc.GetString("autopsy-report-fiber", ("fiber", fiber)));
            found = true;
        }

        return found ? clues.ToString() : Loc.GetString("autopsy-report-forensics-none");
    }

    private string GetPartName(EntityUid part)
    {
        if (!TryComp<BodyPartComponent>(part, out var bodyPart))
            return Name(part);

        return GetPartName(bodyPart.PartType, bodyPart.Symmetry);
    }

    private string GetPartName(BodyPartType type, BodyPartSymmetry symmetry)
    {
        var name = Loc.GetString($"autopsy-part-{type.ToString().ToLowerInvariant()}");

        if (symmetry == BodyPartSymmetry.None)
            return name;

        return Loc.GetString("autopsy-part-side",
            ("part", name),
            ("side", Loc.GetString($"autopsy-side-{symmetry.ToString().ToLowerInvariant()}")));
    }

    private string GetWoundName(EntityUid wound)
    {
        var proto = MetaData(wound).EntityPrototype?.ID;

        return proto != null && Loc.TryGetString($"autopsy-wound-{proto.ToLowerInvariant()}", out var name)
            ? name
            : Name(wound);
    }

    private string GetSeverityName(WoundSeverity severity)
    {
        return Loc.GetString($"autopsy-severity-{severity.ToString().ToLowerInvariant()}");
    }
}
