// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.Shuttles.Components;
using Content.Shared.Xenoborgs.Components;
using Robust.Shared.Map;

namespace Content.Server._Dumont.Xenoborgs;

public sealed partial class XenoborgAssimilationConditionSystem : EntitySystem
{
    [Dependency] private EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private XenoborgsRuleSystem _xenoborgsRule = default!;

    private readonly HashSet<MapId> _centcommMaps = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoborgAssimilationConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(Entity<XenoborgAssimilationConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var assimilation = GetAssimilationProgress();

        if (assimilation < 1f)
        {
            args.Progress = assimilation * ent.Comp.AssimilationWeight;
            return;
        }

        args.Progress = AnyXenoborgAtCentcomm() ? 1f : ent.Comp.AssimilationWeight;
    }

    private float GetAssimilationProgress()
    {
        var target = 0f;
        var query = EntityQueryEnumerator<XenoborgsRuleComponent>();
        while (query.MoveNext(out var rule))
        {
            if (rule.XenoborgShuttleCalled)
                return 1f;

            target = Math.Max(target, rule.XenoborgShuttleCallPercentage);
        }

        if (target <= 0f)
            return 0f;

        return Math.Clamp(_xenoborgsRule.GetXenoborgRatio() / target, 0f, 1f);
    }

    private bool AnyXenoborgAtCentcomm()
    {
        _centcommMaps.Clear();
        var centcommQuery = EntityQueryEnumerator<StationCentcommComponent>();
        while (centcommQuery.MoveNext(out var centcomm))
        {
            if (centcomm.MapEntity is { } map && TryComp<TransformComponent>(map, out var mapXform))
                _centcommMaps.Add(mapXform.MapID);
        }

        var query = EntityQueryEnumerator<XenoborgComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (!_mobState.IsAlive(uid) || !_mind.TryGetMind(uid, out _, out _))
                continue;

            if (_emergencyShuttle.IsTargetEscaping(uid) || _centcommMaps.Contains(xform.MapID))
                return true;

            if (TryComp<FTLComponent>(xform.GridUid, out var ftl)
                && _centcommMaps.Contains(_transform.GetMapId(ftl.TargetCoordinates)))
                return true;
        }

        return false;
    }
}
