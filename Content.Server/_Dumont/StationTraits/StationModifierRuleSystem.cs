using Content.Server.Anomaly;
using Content.Server.GameTicking.Rules;
using Content.Server.Research.Systems;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Research.Components;
using Content.Shared.Station.Components;

namespace Content.Server._Dumont.StationTraits;

public sealed partial class StationModifierRuleSystem : GameRuleSystem<StationModifierRuleComponent>
{
    public float LightEnergy { get; private set; } = 1f;
    public float LightRadius { get; private set; } = 1f;
    public float SolarPower { get; private set; } = 1f;
    public float ResearchPoints { get; private set; } = 1f;
    public bool HeatAlert { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResearchServerComponent, ResearchServerGetPointsPerSecondEvent>(OnGetPointsPerSecond, after: [ typeof(ResearchSystem), typeof(AnomalySystem) ]);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    protected override void Started(EntityUid uid, StationModifierRuleComponent comp, GameRuleComponent rule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, rule, args);

        Refresh();
    }

    protected override void Ended(EntityUid uid, StationModifierRuleComponent comp, GameRuleComponent rule, GameRuleEndedEvent args)
    {
        base.Ended(uid, comp, rule, args);

        Refresh();
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        LightEnergy = 1f;
        LightRadius = 1f;
        SolarPower = 1f;
        ResearchPoints = 1f;
        HeatAlert = false;
    }

    private void OnGetPointsPerSecond(Entity<ResearchServerComponent> ent, ref ResearchServerGetPointsPerSecondEvent args)
    {
        args.Points = ModifyResearchPoints(args.Points);
    }

    public int ModifyResearchPoints(int points)
    {
        return points > 0 ? (int) MathF.Round(points * ResearchPoints) : points;
    }
    public bool ShouldFeelHot(EntityUid uid)
    {
        return HeatAlert && Transform(uid).GridUid is { } grid && HasComp<StationMemberComponent>(grid);
    }

    private void Refresh()
    {
        LightEnergy = 1f;
        LightRadius = 1f;
        SolarPower = 1f;
        ResearchPoints = 1f;
        HeatAlert = false;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var comp, out _))
        {
            LightEnergy *= comp.LightEnergy;
            LightRadius *= comp.LightRadius;
            SolarPower *= comp.SolarPower;
            ResearchPoints *= comp.ResearchPoints;
            HeatAlert |= comp.HeatAlert;
        }
    }
}
