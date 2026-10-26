// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+Baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Triangle <74220687+cwestkaemper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 loltart <159829224+loltart@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._White.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.Audio;
using Content.Shared.Audio;
using Robust.Shared.Audio;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Nuke;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Caste;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Server.Audio; // Goobstation - Play music on announcement
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Station.Components;

namespace Content.Server._White.GameTicking.Rules;

public sealed partial class XenomorphsRuleSystem : GameRuleSystem<XenomorphsRuleComponent>
{
    private static readonly EntProtoId XenomorphSpawnerProto = "SpawnPointGhostXenomorph";

    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _protoManager = default!;
    [Dependency] private IRobustRandom _random = default!;

    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private ServerGlobalSoundSystem _sound = default!; // Dumont

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphsRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagEntitySelected);

        SubscribeLocalEvent<XenomorphComponent, ComponentInit>(OnXenomorphInit);
        SubscribeLocalEvent<XenomorphComponent, BeforeXenomorphEvolutionEvent>(BeforeXenomorphEvolution);
        SubscribeLocalEvent<XenomorphComponent, AfterXenomorphEvolutionEvent>(AfterXenomorphEvolution);

        SubscribeLocalEvent<NukeExplodedEvent>(OnNukeExploded);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);
    }

    private void AfterAntagEntitySelected(
        EntityUid uid,
        XenomorphsRuleComponent component,
        AfterAntagEntitySelectedEvent args
    )
    {
        if (args.Session == null || !Exists(args.EntityUid))
            return;

        if (!component.Xenomorphs.Contains(args.EntityUid)) // Dumont
            component.Xenomorphs.Add(args.EntityUid);
    }

    private void OnXenomorphInit(EntityUid uid, XenomorphComponent component, ComponentInit args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var xenomorphsRule, out _))
        {
            xenomorphsRule.Xenomorphs.Add(uid);
        }
    }

    private void BeforeXenomorphEvolution(
        EntityUid uid,
        XenomorphComponent component,
        BeforeXenomorphEvolutionEvent args
    )
    {
        if (HasComp<Content.Server._Dumont.Xenomorphs.XenomorphLineageComponent>(uid)) return;
        if (!_protoManager.TryIndex(args.Caste, out var cast) || cast.MaxCount == 0)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var xenomorphsRule, out _))
        {
            if (!xenomorphsRule.Xenomorphs.Contains(uid))
                continue;

            if (xenomorphsRule.TotalCastes.GetValueOrDefault(args.Caste) >= cast.MaxCount
                || args.CheckNeedCasteDeath && cast.NeedCasteDeath != null && GetXenomorphs(xenomorphsRule, cast.NeedCasteDeath).Count > 0)
            {
                _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-no-cast-slot", ("caste", Loc.GetString(cast.Name))), uid, uid);
                args.Cancel();
                return;
            }
        }
    }

    private void AfterXenomorphEvolution(
        EntityUid uid,
        XenomorphComponent component,
        AfterXenomorphEvolutionEvent args
    )
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var xenomorphsRule, out _))
        {
            // Dumont start
            if (!xenomorphsRule.Xenomorphs.Remove(uid))
                continue;

            if (!xenomorphsRule.Xenomorphs.Contains(args.EvolvedInto))
                xenomorphsRule.Xenomorphs.Add(args.EvolvedInto);
            xenomorphsRule.TotalCastes[args.Caste] = xenomorphsRule.TotalCastes.GetValueOrDefault(args.Caste) + 1;
            // Dumont end
        }
    }

    private void OnNukeExploded(NukeExplodedEvent ev)
    {
        if (ev.OwningStation == null)
            return;

        var correctStation = false;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var xenomorphs, out _))
        {
            if (xenomorphs.WinConditions.Contains(WinCondition.HiveContactEstablished)
                || EntityManager.System<Content.Server._Dumont.Xenomorphs.XenomorphShuttleHijackSystem>().Invaded) continue;
            foreach (var grid in GetStationGrids())
            {
                if (ev.OwningStation != grid)
                    continue;

                xenomorphs.WinType = WinType.CrewMinor;
                xenomorphs.WinConditions.Add(WinCondition.NukeExplodedOnStation);
                ForceEndSelf(uid);
                correctStation = true;
            }
        }

        if (correctStation)
            _roundEnd.EndRound();
    }

    private void OnGameRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New is not GameRunLevel.PostRound)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var xenomorphs, out _))
        {
            OnRoundEnd(xenomorphs);
            ForceEndSelf(uid);
        }
    }

    private void OnRoundEnd(XenomorphsRuleComponent component)
    {
        if (component.WinConditions.Contains(WinCondition.HiveContactEstablished)) return;
        var invasion = EntityManager.System<Content.Server._Dumont.Xenomorphs.XenomorphShuttleHijackSystem>();
        if (GetXenomorphs(component).Count == 0)
        {
            component.WinType = WinType.CrewMajor;
            if (!component.WinConditions.Contains(WinCondition.AllReproduceXenoDead))
                component.WinConditions.Add(WinCondition.AllReproduceXenoDead);
        }
        else if (invasion.Invaded)
        {
            component.WinType = WinType.XenoMinor;
            if (!component.WinConditions.Contains(WinCondition.XenoInfiltratedOnCentCom))
                component.WinConditions.Add(WinCondition.XenoInfiltratedOnCentCom);
        }
        else if (invasion.CrewArrived && invasion.CrewEscaped)
        {
            component.WinType = WinType.CrewMinor;
            if (!component.WinConditions.Contains(WinCondition.CrewEscaped))
                component.WinConditions.Add(WinCondition.CrewEscaped);
        }
    }

    protected override void AppendRoundEndText(
        EntityUid uid,
        XenomorphsRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args
        )
    {
        var winText = Loc.GetString($"xenomorphs-{component.WinType.ToString().ToLower()}");
        args.AddLine(winText);

        foreach (var cond in component.WinConditions)
        {
            var text = Loc.GetString($"xenomorphs-cond-{cond.ToString().ToLower()}");
            args.AddLine(text);
        }
    }

    protected override void Started(
        EntityUid uid,
        XenomorphsRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args
    )
    {
        base.Started(uid, component, gameRule, args);

        EntityManager.System<Content.Server._Dumont.Xenomorphs.XenomorphHiveCaptureSystem>().StartInfestation();
        component.NextCheck = _timing.CurTime + component.CheckDelay;
        component.AnnouncementTime = _timing.CurTime + _random.Next(component.MinTimeToAnnouncement, component.MaxTimeToAnnouncement);
    }

    protected override void ActiveTick(
        EntityUid uid,
        XenomorphsRuleComponent component,
        GameRuleComponent gameRule,
        float frameTime
    )
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.NextCheck > _timing.CurTime)
            return;

        component.NextCheck = _timing.CurTime + component.CheckDelay;

        if (!component.Announced && component.AnnouncementTime <= _timing.CurTime)
        {
            component.Announced = true;

            if (!string.IsNullOrEmpty(component.Announcement))
                _chat.DispatchGlobalAnnouncement(Loc.GetString(component.Announcement), component.Sender != null ? Loc.GetString(component.Sender) : null, colorOverride: component.AnnouncementColor);

            if (EntityManager.System<Content.Server._Dumont.Xenomorphs.XenomorphHiveObjectivesSystem>().HijackingQueen == null)
                PlayXenomorphMusic(component.XenomorphInfestationSound); // Dumont
        }

        CheckRoundEnd(uid, component, gameRule);
    }

    private void CheckRoundEnd(EntityUid uid, XenomorphsRuleComponent component, GameRuleComponent gameRule)
    {
        var invasion = EntityManager.System<Content.Server._Dumont.Xenomorphs.XenomorphShuttleHijackSystem>();
        if (component.WinConditions.Contains(WinCondition.HiveContactEstablished) || invasion.Invaded) return;
        var stationGrids = GetStationGrids();

        var xenomorphs = GetXenomorphs(component);

        // Check if there are any xenomorph larva ghost role present
        var hasXenomorphSpawners = false;
        var spawnerQuery = AllEntityQuery<GhostRoleComponent, MetaDataComponent>();
        while (spawnerQuery.MoveNext(out var spawnerUid, out _, out var metaData))
        {
            if (metaData.EntityPrototype != null && metaData.EntityPrototype.ID == XenomorphSpawnerProto)
            {
                hasXenomorphSpawners = true;
                break;
            }
        }

        if (xenomorphs.Count == 0 && !hasXenomorphSpawners)
        {
            if (component.Announced && !string.IsNullOrEmpty(component.NoMoreThreatAnnouncement))
                _chat.DispatchGlobalAnnouncement(Loc.GetString(component.NoMoreThreatAnnouncement), component.Sender != null ? Loc.GetString(component.Sender) : null, colorOverride: component.NoMoreThreatAnnouncementColor);

            component.WinType = WinType.CrewMajor;
            component.WinConditions.Add(WinCondition.AllReproduceXenoDead);
            ForceEndSelf(uid, gameRule);
            return; // Dumont
        }

        if (invasion.CommsLocked) return;

        if (xenomorphs.Count / (float) (xenomorphs.Count + GetHumans(stationGrids, true).Count) >= 1)
        {
            component.WinType = WinType.XenoMajor;
            component.WinConditions.Add(WinCondition.AllCrewDead);
            ForceEndSelf(uid, gameRule);
            _roundEnd.EndRound();
            return;
        }
    }

    public void CompleteShuttleInvasion()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var rule, out _))
        {
            rule.WinType = WinType.XenoMinor;
            rule.Announced = true;
            if (!rule.WinConditions.Contains(WinCondition.XenoInfiltratedOnCentCom))
                rule.WinConditions.Add(WinCondition.XenoInfiltratedOnCentCom);
        }
    }

    public void CompleteHiveContact()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var rule, out _))
        {
            rule.WinType = WinType.XenoMajor;
            rule.Announced = true;
            if (!rule.WinConditions.Contains(WinCondition.HiveContactEstablished)) rule.WinConditions.Add(WinCondition.HiveContactEstablished);
        }
    }

    // Dumont start
    private void PlayXenomorphMusic(SoundSpecifier sound)
    {
        foreach (var grid in GetStationGrids())
        {
            _sound.StopStationEventMusic(grid, StationEventMusicType.Xenomorph);
            _sound.DispatchStationEventMusic(grid, sound, StationEventMusicType.Xenomorph, sound.Params);
        }
    }
    // Dumont end

    private List<EntityUid> GetHumans(HashSet<EntityUid>? stationGrids = null, bool includeOffStation = false)
    {
        var humans = new List<EntityUid>();
        stationGrids ??= GetStationGrids();

        var players = AllEntityQuery<HumanoidAppearanceComponent, ActorComponent, MobStateComponent, TransformComponent>();
        while (players.MoveNext(out var uid, out _, out _, out var mobStateComponent, out var xform))
        {
            if (_mobState.IsDead(uid, mobStateComponent)
                || !includeOffStation && !stationGrids.Contains(xform.GridUid ?? EntityUid.Invalid))
                continue;

            humans.Add(uid);
        }

        return humans;
    }

    private List<EntityUid> GetXenomorphs(XenomorphsRuleComponent xenomorphsRule, ProtoId<XenomorphCastePrototype>? cast = null)
    {
        var xenomorphs = new List<EntityUid>();

        foreach (var xenomorph in xenomorphsRule.Xenomorphs.ToList())
        {
            if (!Exists(xenomorph) || !TryComp<XenomorphComponent>(xenomorph, out var xenomorphComponent))
            {
                xenomorphsRule.Xenomorphs.Remove(xenomorph);
                continue;
            }

            if (_mobState.IsDead(xenomorph) || cast.HasValue && xenomorphComponent.Caste != cast)
                continue;

            xenomorphs.Add(xenomorph);
        }

        return xenomorphs;
    }

    private HashSet<EntityUid> GetStationGrids()
    {
        var stationGrids = new HashSet<EntityUid>();
        foreach (var station in _gameTicker.GetSpawnableStations())
        {
            if (TryComp<StationDataComponent>(station, out var _) && _station.GetLargestGrid(station) is { } grid)
                stationGrids.Add(grid);
        }

        return stationGrids;
    }
}
