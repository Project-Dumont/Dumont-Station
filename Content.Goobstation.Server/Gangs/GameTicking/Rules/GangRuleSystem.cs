using System.Linq;
using Content.Goobstation.Shared.Gangs;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Content.Server.Roles;
using Content.Server.Spawners.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Pinpointer;
using Content.Shared.Radio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Gangs.GameTicking.Rules;

public sealed class GangRuleSystem : GameRuleSystem<GangRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GangRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<GangMemberComponent, GetBriefingEvent>(OnGetMemberBrief);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<GangLeaderComponent, MobStateChangedEvent>(OnLeaderMobStateChanged);
    }

    #region Crate Drop System

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GangRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var gangRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleActive(uid, gameRule))
                continue;

            gangRule.Accumulator += frameTime;
            if (!gangRule.Announced && gangRule.Accumulator >= gangRule.DropInterval - gangRule.WarningTime)
                AnnounceDrop(uid, gangRule);
        }
    }

    private void AnnounceDrop(EntityUid uid, GangRuleComponent comp)
    {
        comp.Announced = true;
        comp.DropLocation = FindLocation();

        // this spawns a marker that spawns a gang crate after despawn
        var marker = Spawn("SpawnSupplypodAnimation", comp.DropLocation.Value);

        var locationStr = FormattedMessage.RemoveMarkupOrThrow(
            _navMap.GetNearestBeaconString(marker));

        var message = Loc.GetString("gang-drop-announcement", ("location", locationStr));
        SendGangRadioMessage(uid, message, comp.ChannelId);
        comp.Accumulator = 0f;
        comp.Announced = false;
        comp.DropLocation = null;
    }



    private void SendGangRadioMessage(EntityUid sourceUid, string message, string channelId)
    {
        if (!_proto.TryIndex<RadioChannelPrototype>(channelId, out var channel))
            return;

        _radio.SendRadioMessage(
            messageSource: sourceUid,
            message: message,
            channel: channel,
            radioSource: sourceUid,
            language: null, // it works okay
            escapeMarkup: true
        );
    }

    private EntityCoordinates FindLocation()
    {
        var beacons = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<NavMapBeaconComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var xform))
        {
            beacons.Add(xform.Coordinates);
        }

        if (beacons.Count > 0)
            return _random.Pick(beacons);

        var spawnPoints = new List<EntityCoordinates>();
        var spawnQuery = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        while (spawnQuery.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (spawnPoint.SpawnType != SpawnPointType.LateJoin)
                continue;

            spawnPoints.Add(xform.Coordinates);
        }

        if (spawnPoints.Count > 0)
            return _random.Pick(spawnPoints);

        // fallback
        return new EntityCoordinates();
    }

    private void PrepareDropLocation(GangRuleComponent comp)
    {
        // choosing the location
        comp.DropLocation = FindLocation();
    }


    #endregion

    #region Game Rule Stuff

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New == GameRunLevel.InRound)
        {
            ResetDropTimers();
        }
    }

    private void ResetDropTimers()
    {
        var query = EntityQueryEnumerator<GangRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var gangRule, out var gameRule))
        {
            if (!GameTicker.IsGameRuleActive(uid, gameRule))
                continue;
            gangRule.Accumulator = 0f;
            gangRule.Announced = false;
            gangRule.DropLocation = null;
        }
    }

    private void OnSelectAntag(EntityUid uid, GangRuleComponent comp, AfterAntagEntitySelectedEvent args)
    {
        MakeGangLeader(args.EntityUid, comp);
    }

    public bool MakeGangLeader(EntityUid uid, GangRuleComponent component)
    {
        // creating the gang with the id
        var gangId = uid;

        if (component.AvailableHatTypes.Count == 0)
            return false; // womp womp on people that get the seventh gang leader (7 is too much already)

        var hatType = _random.PickAndTake(component.AvailableHatTypes);
        component.GangHatPreferences[gangId] = hatType;

        var leaderComp = EnsureComp<GangLeaderComponent>(uid);
        leaderComp.GangId = gangId;

        var memberComp = EnsureComp<GangMemberComponent>(uid);
        memberComp.GangId = gangId;
        leaderComp.Members.Add(uid);

        var briefing = Loc.GetString("gang-leader-antag-greeter");
        _antag.SendBriefing(uid, briefing, Color.Yellow, component.BriefingSound);

        return true;
    }

    private void OnGetMemberBrief(Entity<GangMemberComponent> comp, ref GetBriefingEvent args)
    {
        if (args.Mind.Comp.OwnedEntity is { } entity)
            args.Append(MakeMemberBriefing(entity));
    }

    private string MakeMemberBriefing(EntityUid entity)
    {
        return Loc.GetString("gang-member-antag-greeter");
    }

    protected override void AppendRoundEndText(EntityUid uid,
        GangRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var gangs = new Dictionary<EntityUid, List<string>>();
        var gangLeaders = new Dictionary<EntityUid, string>();
        var gangSignCounts = new Dictionary<EntityUid, int>();
        var graffitiQuery = EntityQueryEnumerator<GangGraffitiComponent>();

        // counts the graffities by gangs to display in the manifest
        while (graffitiQuery.MoveNext(out _, out var graffiti))
        {
            var gangId = graffiti.GangId;

            if (gangId == null
                || !gangId.Value.IsValid())
                continue;

            var id = gangId.Value;

            gangSignCounts.TryGetValue(id, out var count);
            gangSignCounts[id] = count + 1;
        }

        var memberQuery = EntityQueryEnumerator<GangMemberComponent, MetaDataComponent>();
        while (memberQuery.MoveNext(out var entity, out var member, out var meta))
        {
            var gangId = member.GangId;
            var name = meta.EntityName;

            if (!gangs.ContainsKey(gangId))
                gangs[gangId] = new List<string>();

            gangs[gangId].Add(name);

            if (HasComp<GangLeaderComponent>(entity))
                gangLeaders[gangId] = name;
        }

        foreach (var (gangId, members) in gangs)
        {
            gangSignCounts.TryGetValue(gangId, out var signCount);
            var signText = Loc.GetString("gang-signs-count", ("count", signCount));

            if (gangLeaders.TryGetValue(gangId, out var leaderName))
                args.AddLine(Loc.GetString("gang-gang-led-by", ("leader", leaderName), ("signs", signText)));

            else
                args.AddLine(Loc.GetString("gang-gang-no-leader", ("signs", signText)));

            args.AddLine(Loc.GetString("gang-members-header"));
            foreach (var member in members)
            {
                args.AddLine($"- {member}");
            }
            args.AddLine(""); // peak
        }
    }
    public string? GetHatProto(EntityUid gangId)
    {
        var query = EntityQueryEnumerator<GangRuleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.GangHatPreferences.TryGetValue(gangId, out var hat))
                return hat;
        }
        return null;
    }

    private void OnLeaderMobStateChanged(EntityUid uid, GangLeaderComponent leader, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        TransferLeadership(uid, leader);
    }

    private void TransferLeadership(EntityUid oldLeader, GangLeaderComponent leaderComp)
    {
        var gangId = leaderComp.GangId;
        var possibleLeaders = new List<EntityUid>();

        var query = EntityQueryEnumerator<GangMemberComponent, MobStateComponent>();
        while (query.MoveNext(out var memberUid, out var memberComp, out var mobState))
        {
            if (memberComp.GangId != gangId
                || memberUid == oldLeader
                || !_mobState.IsAlive(memberUid, mobState))
                continue;

            possibleLeaders.Add(memberUid);
        }
        if (possibleLeaders.Count == 0)
        {
            RemComp<GangLeaderComponent>(oldLeader);
            return;
        }

        var newLeader = _random.Pick(possibleLeaders);
        var newLeaderName = MetaData(newLeader).EntityName;
        RemComp<GangLeaderComponent>(oldLeader);

        var newLeaderComp = AddComp<GangLeaderComponent>(newLeader);
        newLeaderComp.GangId = gangId;
        newLeaderComp.Members = leaderComp.Members;

        foreach (var member in newLeaderComp.Members)
        {
            if (!Exists(member))
                continue;

            _popup.PopupEntity(Loc.GetString("gang-new-leader-announcement", ("name", newLeaderName)),member,member, PopupType.MediumCaution);
        }
    }

    #endregion
}
