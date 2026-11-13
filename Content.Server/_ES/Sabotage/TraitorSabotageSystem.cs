// SPDX-FileCopyrightText: 2025-2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Server._ES.Objectives;
using Content.Shared.Access.Systems;
using Robust.Shared.Timing;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Server.Destructible;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Shared._ES.Sabotage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Content.Goobstation.Common.Effects;
using Robust.Shared.Random;

namespace Content.Server._ES.Sabotage;

public sealed partial class TraitorSabotageSystem : EntitySystem
{
    [Dependency] private AccessReaderSystem _access = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private CodeConditionSystem _code = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SparksSystem _sparks = default!;
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SabotageConditionComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<SabotageTargetComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<SabotageTargetComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SabotageTargetComponent, SabotageDoAfterEvent>(OnSabotage);
        SubscribeLocalEvent<SabotageTargetComponent, RepairSabotageDoAfterEvent>(OnRepair);
        SubscribeLocalEvent<CommunicationsConsoleComponent, SabotageEffectEvent>(OnCommunicationsSabotage);
        InitializeTelecom();
        InitializeNavigation();
    }

    partial void InitializeTelecom();
    partial void InitializeNavigation();

    private bool IsOperational(EntityUid target, bool needsPower = true)
    {
        return !TerminatingOrDeleted(target)
            && Transform(target).Anchored
            && (!TryComp<DestructibleComponent>(target, out var destructible) || !destructible.IsBroken)
            && (!needsPower || !TryComp<ApcPowerReceiverComponent>(target, out var power)
                || (power.Powered && !power.PowerDisabled));
    }

    private void OnAssigned(Entity<SabotageConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        var station = _station.GetOwningStation(args.Mind.OwnedEntity);
        ent.Comp.Station = station;
        if (station == null)
        {
            args.Cancelled = true;
            return;
        }

        var query = EntityQueryEnumerator<SabotageTargetComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_station.GetOwningStation(uid) == station && IsOperational(uid)
                && _whitelist.IsWhitelistPass(ent.Comp.Whitelist, uid))
                return;
        }
        args.Cancelled = true;
    }

    private bool MatchesObjective(EntityUid user, EntityUid target, EntityUid objective)
    {
        return TryComp<SabotageConditionComponent>(objective, out var condition)
            && !_code.IsCompleted(objective)
            && condition.Station != null
            && _station.GetOwningStation(user) == condition.Station
            && _station.GetOwningStation(target) == condition.Station
            && _whitelist.IsWhitelistPass(condition.Whitelist, target);
    }

    private bool CanSabotage(EntityUid user, Entity<SabotageTargetComponent> target)
    {
        if (target.Comp.Sabotaged || !IsOperational(target)
            || !_mind.TryGetMind(user, out _, out var mind))
            return false;

        foreach (var objective in mind.Objectives)
        {
            if (MatchesObjective(user, target, objective))
                return true;
        }
        return false;
    }

    private void OnGetVerbs(Entity<SabotageTargetComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !args.CanComplexInteract)
            return;

        var user = args.User;
        if (ent.Comp.Sabotaged)
        {
            if (!CanRepair(user, ent.Comp))
                return;
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("es-sabotage-repair"),
                DoContactInteraction = true,
                Act = () => _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.RepairTime,
                    new RepairSabotageDoAfterEvent(), ent, ent)
                {
                    BreakOnMove = true,
                    BreakOnDamage = true,
                    NeedHand = true,
                    BlockDuplicate = true,
                    DuplicateCondition = DuplicateConditions.SameEvent,
                }),
            });
            return;
        }
        if (!CanSabotage(user, ent))
            return;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("es-sabotage-verb"),
            DoContactInteraction = true,
            Act = () =>
            {
                if (!CanSabotage(user, ent))
                    return;
                if (_doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.SabotageTime,
                        new SabotageDoAfterEvent(), ent, ent)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = true,
                        BlockDuplicate = true,
                        DuplicateCondition = DuplicateConditions.SameEvent,
                    }, out var id))
                {
                    ent.Comp.ActiveDoAfters.Add(id.Value);
                    _popup.PopupEntity(Loc.GetString("es-sabotage-start"), ent, user);
                }
            },
        });
    }

    private void OnExamined(Entity<SabotageTargetComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Sabotaged)
            args.PushMarkup(Loc.GetString("es-sabotage-damaged"));
        else if (CanSabotage(args.Examiner, ent))
            args.PushMarkup(Loc.GetString("es-sabotage-hint"));
    }

    private void OnSabotage(Entity<SabotageTargetComponent> ent, ref SabotageDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !CanSabotage(args.User, ent))
            return;

        var effect = new SabotageEffectEvent(args.User);
        RaiseLocalEvent(ent, ref effect);
        if (!effect.Handled)
            return;

        args.Handled = true;
        if (_mind.TryGetMind(args.User, out _, out var mind))
        {
            foreach (var objective in mind.Objectives)
            {
                if (MatchesObjective(args.User, ent, objective))
                    _code.SetCompleted(objective);
            }
        }
        _sparks.DoSparks(Transform(ent).Coordinates);
        _popup.PopupEntity(Loc.GetString("es-sabotage-success"), ent, args.User);
        _adminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.User):player} sabotaged {ToPrettyString(ent)}.");
    }

    private void OnRepair(Entity<SabotageTargetComponent> ent, ref RepairSabotageDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !ent.Comp.Sabotaged || !CanRepair(args.User, ent.Comp) || !IsOperational(ent, needsPower: false))
            return;

        ent.Comp.Sabotaged = false;
        args.Handled = true;
        _sparks.DoSparks(Transform(ent).Coordinates);
        _popup.PopupEntity(Loc.GetString("es-sabotage-repaired"), ent, args.User);
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(args.User):player} repaired sabotage on {ToPrettyString(ent)}.");
        var ev = new SabotageRepairedEvent();
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnCommunicationsSabotage(Entity<CommunicationsConsoleComponent> ent, ref SabotageEffectEvent args)
    {
        if (args.Handled || !TryComp<SabotageTargetComponent>(ent, out var sabotage))
            return;

        _chat.DispatchStationAnnouncement(ent, Loc.GetString("es-sabotage-comms-message"),
            Loc.GetString("es-sabotage-comms-sender"), colorOverride: Color.DarkGray);
        sabotage.Sabotaged = true;
        args.Handled = true;
    }

    public void ModifyAnnouncement(EntityUid source, ref string message, ref string? sender)
    {
        var station = _station.GetOwningStation(source);
        if (station == null)
            return;

        var query = EntityQueryEnumerator<SabotageTargetComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Sabotaged || !HasComp<CommunicationsConsoleComponent>(uid) || _station.GetOwningStation(uid) != station)
                continue;
            message = DistortMessage(message, 0.33f);
            sender = Loc.GetString("es-sabotage-comms-sender");
            return;
        }
    }

    private bool CanRepair(EntityUid user, SabotageTargetComponent comp)
    {
        foreach (var access in _access.FindAccessTags(user))
        {
            if (comp.RepairAccess.Contains(access))
                return true;
        }
        return false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<SabotageTargetComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.ActiveDoAfters.RemoveWhere(id => !_doAfter.IsRunning(id));
            if (comp.ActiveDoAfters.Count == 0 || _timing.CurTime < comp.NextSpark)
                continue;

            comp.NextSpark = _timing.CurTime + TimeSpan.FromSeconds(1);
            _sparks.DoSparks(Transform(uid).Coordinates);
        }
    }

    private string DistortMessage(string message, float chance)
    {
        var chars = message.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (!char.IsWhiteSpace(chars[i]) && _random.Prob(chance))
                chars[i] = '*';
        }
        return new string(chars);
    }
}

[ByRefEvent]
public readonly record struct SabotageRepairedEvent;
