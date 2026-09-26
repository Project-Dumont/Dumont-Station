// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Dumont.Communications;
using Content.Server._Dumont.Shuttles;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Radio;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Destructible;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Radio.Components;
using Content.Shared.Shuttles.Components;
using Content.Shared.Xenoborgs.Components;
using Robust.Shared.Timing;

namespace Content.Server.GameTicking.Rules;

public sealed partial class XenoborgsRuleSystem : GameRuleSystem<XenoborgsRuleComponent>
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private ChatSystem _chatSystem = default!;
    [Dependency] private CommunicationsConsoleSystem _commsConsole = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    [Dependency] private SharedMindSystem _mindSystem = default!;
    [Dependency] private ShuttleSystem _shuttle = default!;
    [Dependency] private StationSystem _station = default!;

    private static readonly Color AnnouncmentColor = Color.Gold;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RadioReceiveAttemptEvent>(OnRadioReceiveAttempt);
        SubscribeLocalEvent<CommunicationsConsoleAnnounceAttemptEvent>(OnCommsAnnounceAttempt);
        SubscribeLocalEvent<EmergencyShuttleDepartingEvent>(OnEmergencyShuttleDeparting);
    }

    private void OnRadioReceiveAttempt(ref RadioReceiveAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var rule, out _))
        {
            if (!rule.CommsBlackout || !rule.BlackoutChannels.Contains(args.Channel.ID))
                continue;

            if (rule.BlackoutOnlyHeadsets
                && !HasComp<HeadsetComponent>(args.RadioReceiver)
                && !HasComp<HeadsetComponent>(args.RadioSource))
                continue;

            args.Cancelled = true;
            return;
        }
    }

    private void OnCommsAnnounceAttempt(ref CommunicationsConsoleAnnounceAttemptEvent args)
    {
        if (args.Cancelled || _station.GetOwningStation(args.Console) == null)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var rule, out _))
        {
            if (!rule.CommsBlackout || !rule.BlackoutCommsConsoles)
                continue;

            args.Cancelled = true;
            return;
        }
    }

    private void OnEmergencyShuttleDeparting(ref EmergencyShuttleDepartingEvent args)
    {
        EntityUid? centcomm = null;
        var centcommQuery = EntityQueryEnumerator<StationCentcommComponent>();
        while (centcommQuery.MoveNext(out var comp))
        {
            if (comp.Entity is { } entity && !Deleted(entity))
            {
                centcomm = entity;
                break;
            }
        }

        if (centcomm == null)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var rule, out _))
        {
            if (!rule.MothershipToCentcomm || !TryComp<RuleGridsComponent>(uid, out var ruleGrids))
                continue;

            foreach (var grid in ruleGrids.MapGrids)
            {
                if (Deleted(grid) || HasComp<FTLComponent>(grid) || !TryComp<ShuttleComponent>(grid, out var shuttle))
                    continue;

                _shuttle.FTLToDock(grid, shuttle, centcomm.Value, args.StartupTime, args.TransitTime);
            }
        }
    }

    public void SendXenoborgDeathAnnouncement(Entity<XenoborgsRuleComponent> ent, bool mothershipCoreAlive)
    {
        if (ent.Comp.MothershipCoreDeathAnnouncmentSent)
            return;

        var status = mothershipCoreAlive ? "alive" : "dead";
        _chatSystem.DispatchGlobalAnnouncement(
            Loc.GetString($"xenoborgs-no-more-threat-mothership-core-{status}-announcement"),
            colorOverride: AnnouncmentColor);
    }

    public void SendMothershipDeathAnnouncement(Entity<XenoborgsRuleComponent> ent)
    {
        _chatSystem.DispatchGlobalAnnouncement(
            Loc.GetString("mothership-destroyed-announcement"),
            colorOverride: AnnouncmentColor);

        ent.Comp.MothershipCoreDeathAnnouncmentSent = true;
    }

    // TODO: Refactor the end of round text
    protected override void AppendRoundEndText(EntityUid uid,
        XenoborgsRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var numXenoborgs = GetNumberXenoborgs();
        var numHumans = _mindSystem.GetAliveHumans().Count;

        if (numXenoborgs < 5)
            args.AddLine(Loc.GetString("xenoborgs-crewmajor"));
        else if (4 * numXenoborgs < numHumans)
            args.AddLine(Loc.GetString("xenoborgs-crewmajor"));
        else if (2 * numXenoborgs < numHumans)
            args.AddLine(Loc.GetString("xenoborgs-crewminor"));
        else if (1.5 * numXenoborgs < numHumans)
            args.AddLine(Loc.GetString("xenoborgs-neutral"));
        else if (numXenoborgs < numHumans)
            args.AddLine(Loc.GetString("xenoborgs-borgsminor"));
        else
            args.AddLine(Loc.GetString("xenoborgs-borgsmajor"));

        var numMothershipCores = GetNumberMothershipCores();

        if (numMothershipCores == 0)
            args.AddLine(Loc.GetString("xenoborgs-cond-all-xenoborgs-dead-core-dead"));
        else if (numXenoborgs == 0)
            args.AddLine(Loc.GetString("xenoborgs-cond-all-xenoborgs-dead-core-alive"));
        else
        {
            args.AddLine(Loc.GetString("xenoborg-number-xenoborg-alive-end", ("count", numXenoborgs)));
            args.AddLine(Loc.GetString("xenoborg-number-crew-alive-end", ("count", numHumans)));
        }

        args.AddLine(Loc.GetString("xenoborg-max-number", ("count", component.MaxNumberXenoborgs)));

        args.AddLine(Loc.GetString("xenoborgs-list-start"));

        var antags = _antag.GetAntagIdentifiers(uid);

        foreach (var (_, sessionData, name) in antags)
        {
            args.AddLine(Loc.GetString("xenoborgs-list", ("name", name), ("user", sessionData.UserName)));
        }
        args.AddLine("");
    }

    public float GetXenoborgRatio()
    {
        var numXenoborgs = GetNumberXenoborgs();
        var total = numXenoborgs + _mindSystem.GetAliveHumans().Count;

        return total == 0 ? 0f : (float) numXenoborgs / total;
    }

    private void CheckRoundEnd(XenoborgsRuleComponent xenoborgsRuleComponent)
    {
        var numXenoborgs = GetNumberXenoborgs();

        xenoborgsRuleComponent.MaxNumberXenoborgs = Math.Max(xenoborgsRuleComponent.MaxNumberXenoborgs, numXenoborgs);

        if (xenoborgsRuleComponent.XenoborgShuttleCalled
            || GetXenoborgRatio() <= xenoborgsRuleComponent.XenoborgShuttleCallPercentage
            || _roundEnd.IsRoundEndRequested())
            return;

        _roundEnd.RequestRoundEnd(null, false, xenoborgsRuleComponent.ShuttleCallText, cantRecall: true);
        xenoborgsRuleComponent.XenoborgShuttleCalled = true;
        xenoborgsRuleComponent.MothershipAnnouncementTime = _timing.CurTime + xenoborgsRuleComponent.MothershipAnnouncementDelay;
    }

    private void SendMothershipAnnouncement(XenoborgsRuleComponent component)
    {
        component.MothershipAnnouncementTime = null;
        component.CommsBlackout = true;
        _commsConsole.UpdateCommsConsoleInterface();

        _chatSystem.DispatchGlobalAnnouncement(
            Loc.GetString(component.MothershipAnnouncement),
            Loc.GetString(component.MothershipAnnouncementSender),
            component.MothershipAnnouncementSound != null,
            component.MothershipAnnouncementSound,
            component.MothershipAnnouncementColor);
    }

    protected override void Started(EntityUid uid, XenoborgsRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        component.NextRoundEndCheck = _timing.CurTime + component.EndCheckDelay;
    }

    protected override void ActiveTick(EntityUid uid, XenoborgsRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.MothershipAnnouncementTime <= _timing.CurTime)
            SendMothershipAnnouncement(component);

        if (!component.NextRoundEndCheck.HasValue || component.NextRoundEndCheck > _timing.CurTime)
            return;

        CheckRoundEnd(component);
        component.NextRoundEndCheck = _timing.CurTime + component.EndCheckDelay;
    }

    /// <summary>
    /// Get the number of xenoborgs
    /// </summary>
    /// <param name="playerControlled">if it should only include xenoborgs with a mind</param>
    /// <param name="alive">if it should only include xenoborgs that are alive</param>
    /// <returns>the number of xenoborgs</returns>
    private int GetNumberXenoborgs(bool playerControlled = true, bool alive = true)
    {
        var numberXenoborgs = 0;

        var query = EntityQueryEnumerator<XenoborgComponent>();
        while (query.MoveNext(out var xenoborg, out _))
        {
            if (HasComp<MothershipCoreComponent>(xenoborg))
                continue;

            if (playerControlled && !_mindSystem.TryGetMind(xenoborg, out _, out _))
                continue;

            if (alive && !_mobState.IsAlive(xenoborg))
                continue;

            numberXenoborgs++;
        }

        return numberXenoborgs;
    }

    /// <summary>
    /// Gets the number of xenoborg cores
    /// </summary>
    /// <returns>the number of xenoborg cores</returns>
    private int GetNumberMothershipCores()
    {
        var numberMothershipCores = 0;

        var mothershipCoreQuery = EntityQueryEnumerator<MothershipCoreComponent>();
        while (mothershipCoreQuery.MoveNext(out _, out _))
        {
            numberMothershipCores++;
        }

        return numberMothershipCores;
    }
}
