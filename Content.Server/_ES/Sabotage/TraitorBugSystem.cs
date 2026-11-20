// SPDX-FileCopyrightText: 2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Goobstation.Common.Effects;
using Content.Server._ES.Objectives;
using Content.Server.Administration.Logs;
using Content.Server.Destructible;
using Content.Server.Objectives.Systems;
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Events;
using Content.Server.Chat.Systems;
using Content.Shared._ES.Sabotage;
using Content.Shared.APC;
using Content.Trauma.Shared.Areas;
using Content.Shared.Database;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._ES.Sabotage;

public sealed partial class TraitorBugSystem : EntitySystem
{
    [Dependency] private GreytideVirusRule _greytide = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private AreaSystem _area = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private CodeConditionSystem _code = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SparksSystem _sparks = default!;
    [Dependency] private IAdminLogManager _adminLog = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TraitorBugConditionComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<TraitorBugConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<TraitorBuggableComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<TraitorBuggableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TraitorBuggableComponent, PlantTraitorBugDoAfterEvent>(OnPlanted);
        SubscribeLocalEvent<TraitorBuggableComponent, RemoveTraitorBugDoAfterEvent>(OnRemoved);
        SubscribeLocalEvent<TraitorBuggableComponent, BreakageEventArgs>(OnBroken);
    }

    private ProtoId<DepartmentPrototype>? GetDepartment(EntityUid target)
    {
        return _area.GetArea(target) is {} area ? _area.GetAreaDepartment(area) : null;
    }

    private bool IsOperational(EntityUid target)
    {
        return !TerminatingOrDeleted(target) && Transform(target).Anchored
            && TryComp<ApcComponent>(target, out var apc) && apc.MainBreakerEnabled
            && (!TryComp<DestructibleComponent>(target, out var destructible) || !destructible.IsBroken);
    }

    private void OnAssigned(Entity<TraitorBugConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        ent.Comp.Station = _station.GetOwningStation(args.Mind.OwnedEntity);
        var departments = new HashSet<ProtoId<DepartmentPrototype>>();
        if (ent.Comp.Station != null)
        {
            var query = EntityQueryEnumerator<TraitorBuggableComponent, ApcComponent>();
            while (query.MoveNext(out var uid, out _, out _))
            {
                if (IsOperational(uid) && _station.GetOwningStation(uid) == ent.Comp.Station
                    && GetDepartment(uid) is {} department && ent.Comp.DepartmentAccessGroups.ContainsKey(department))
                    departments.Add(department);
            }
        }
        if (departments.Count == 0)
        {
            args.Cancelled = true;
            return;
        }
        ent.Comp.Department = _random.Pick(departments);
    }

    private void OnAfterAssign(Entity<TraitorBugConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (ent.Comp.Department is {} department)
            _metaData.SetEntityName(ent, Loc.GetString(department.Id == "Silicon" ? "es-bug-ai-objective-title" : "es-bug-objective-title",
                ("department", DepartmentName(department))), args.Meta);
    }

    private string DepartmentName(ProtoId<DepartmentPrototype> department)
    {
        return department.Id == "Silicon" ? Loc.GetString("es-bug-ai-department")
            : Loc.GetString(_prototype.Index(department).Name);
    }

    private bool Matches(EntityUid objective, EntityUid target)
    {
        return TryComp<TraitorBugConditionComponent>(objective, out var condition)
            && !_code.IsCompleted(objective) && condition.Department != null
            && condition.Station != null && condition.Station == _station.GetOwningStation(target)
            && condition.Department == GetDepartment(target);
    }

    private bool CanBug(EntityUid user, Entity<TraitorBuggableComponent> target)
    {
        if (target.Comp.Objectives.Count != 0 || target.Comp.Planting != null || !IsOperational(target)
            || _station.GetOwningStation(user) != _station.GetOwningStation(target)
            || !_mind.TryGetMind(user, out _, out var mind))
            return false;

        foreach (var objective in mind.Objectives)
        {
            if (Matches(objective, target))
                return true;
        }
        return false;
    }

    private void OnGetVerbs(Entity<TraitorBuggableComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !args.CanComplexInteract)
            return;

        var user = args.User;
        if (ent.Comp.Objectives.Count != 0)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("es-bug-remove"),
                DoContactInteraction = true,
                Act = () => _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.RemoveTime,
                    new RemoveTraitorBugDoAfterEvent(), ent, ent)
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

        if (!CanBug(user, ent))
            return;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("es-bug-plant"),
            DoContactInteraction = true,
            Act = () =>
            {
                if (!CanBug(user, ent))
                    return;
                if (_doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.PlantTime,
                    new PlantTraitorBugDoAfterEvent(), ent, ent)
                {
                    BreakOnMove = true,
                    BreakOnDamage = true,
                    NeedHand = true,
                    BlockDuplicate = true,
                    DuplicateCondition = DuplicateConditions.SameEvent,
                }, out var id))
                {
                    ent.Comp.Planting = id;
                    _appearance.SetData(ent, ApcVisuals.TraitorBugging, true);
                }
            },
        });
    }

    private void OnExamined(Entity<TraitorBuggableComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Objectives.Count != 0)
        {
            var duration = (ent.Comp.EndsAt - ent.Comp.StartedAt).TotalSeconds;
            var progress = duration <= 0 ? 1 : Math.Clamp((_timing.CurTime - ent.Comp.StartedAt).TotalSeconds / duration, 0, 1);
            args.PushMarkup(Loc.GetString("es-bug-progress", ("progress", (int) (progress * 100))));
        }
        else if (CanBug(args.Examiner, ent))
            args.PushMarkup(Loc.GetString("es-bug-hint"));
    }

    private void OnPlanted(Entity<TraitorBuggableComponent> ent, ref PlantTraitorBugDoAfterEvent args)
    {
        if (args.Handled || ent.Comp.Planting != args.DoAfter.Id)
            return;
        ClearPlanting(ent);
        if (args.Cancelled || !CanBug(args.User, ent)
            || !_mind.TryGetMind(args.User, out _, out var mind))
            return;

        foreach (var objective in mind.Objectives)
        {
            if (Matches(objective, ent))
                ent.Comp.Objectives.Add(objective);
        }
        ent.Comp.StartedAt = _timing.CurTime;
        ent.Comp.EndsAt = _timing.CurTime + ent.Comp.Duration;
        args.Handled = true;
        _sparks.DoSparks(Transform(ent).Coordinates);
        _popup.PopupEntity(Loc.GetString("es-bug-planted"), ent, args.User);
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(args.User):player} planted a bug on {ToPrettyString(ent)}.");
    }

    private void OnRemoved(Entity<TraitorBuggableComponent> ent, ref RemoveTraitorBugDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.Objectives.Count == 0)
            return;
        ent.Comp.Objectives.Clear();
        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("es-bug-removed"), ent, args.User);
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(args.User):player} removed a bug from {ToPrettyString(ent)}.");
    }

    private void OnBroken(Entity<TraitorBuggableComponent> ent, ref BreakageEventArgs args)
    {
        ent.Comp.Objectives.Clear();
        ClearPlanting(ent, cancel: true);
    }

    private void ClearPlanting(Entity<TraitorBuggableComponent> ent, bool cancel = false)
    {
        var id = ent.Comp.Planting;
        ent.Comp.Planting = null;
        _appearance.SetData(ent, ApcVisuals.TraitorBugging, false);
        if (cancel)
            _doAfter.Cancel(id);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<TraitorBuggableComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Planting != null && (!_doAfter.IsRunning(comp.Planting) || !IsOperational(uid)))
                ClearPlanting((uid, comp), cancel: true);
            if (comp.Objectives.Count == 0)
                continue;
            if (!IsOperational(uid))
            {
                comp.Objectives.Clear();
                continue;
            }
            if (_timing.CurTime < comp.EndsAt)
                continue;

            // Only credit the objectives captured when this bug was planted.
            foreach (var objective in comp.Objectives)
            {
                if (TerminatingOrDeleted(objective) || !Matches(objective, uid))
                    continue;

                var condition = Comp<TraitorBugConditionComponent>(objective);
                if (condition.Department is not {} department || condition.Station is not {} station
                    || !condition.DepartmentAccessGroups.TryGetValue(department, out var group))
                    continue;

                var affected = _greytide.ApplyVirus(station, new[] { group }, condition.Blacklist, department);
                _adminLog.Add(LogType.Action, LogImpact.High,
                    $"APC bug at {ToPrettyString(uid)} compromised {affected} doors/lockers in {department}.");
                if (affected == 0)
                    continue;

                _chat.DispatchStationAnnouncement(uid, Loc.GetString("es-bug-network-failure",
                    ("department", DepartmentName(department))));
                _code.SetCompleted(objective);
            }
            comp.Objectives.Clear();
            _sparks.DoSparks(Transform(uid).Coordinates);
        }
    }
}
