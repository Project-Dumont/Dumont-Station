using System.Linq;
using System.Text;
using Content.Shared._Dumont.Coroner;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Part;
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
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        EnsureComp<TimeOfDeathComponent>(args.Target, out var timeOfDeath);
        timeOfDeath.Time = _timing.CurTime;
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
        report.Append(GetTimeOfDeath(target));
        report.Append('\n');
        report.Append(GetDamage(target));
        report.Append('\n');
        report.Append(GetWounds(target));

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
                     .Where(wound => !wound.Comp.IsScar)
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

    private string GetPartName(EntityUid part)
    {
        if (!TryComp<BodyPartComponent>(part, out var bodyPart))
            return Name(part);

        var name = Loc.GetString($"autopsy-part-{bodyPart.PartType.ToString().ToLowerInvariant()}");

        if (bodyPart.Symmetry == BodyPartSymmetry.None)
            return name;

        return Loc.GetString("autopsy-part-side",
            ("part", name),
            ("side", Loc.GetString($"autopsy-side-{bodyPart.Symmetry.ToString().ToLowerInvariant()}")));
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
