using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Revolutionary;

public sealed class RevForcesFollowerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevForcesFollowerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<RevForcesFollowerComponent> ent, ref MapInitEvent args)
    {
        _npc.SetBlackboard(ent, NPCBlackboard.FollowTarget, new EntityCoordinates(ent, 0, 0));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<RevForcesFollowerComponent, HTNComponent>();
        while (query.MoveNext(out var uid, out var comp, out var htn))
        {
            if (comp.NextCheck > now)
                continue;

            comp.NextCheck = now + comp.CheckInterval;

            if (_mobState.IsDead(uid))
                continue;

            if (IsValidTarget(comp.Target))
                continue;

            comp.Target = FindClosestHeadRev(uid);
            _npc.SetBlackboard(uid, NPCBlackboard.FollowTarget, new EntityCoordinates(comp.Target ?? uid, 0, 0), htn);
        }
    }

    private bool IsValidTarget(EntityUid? target)
    {
        return target is { } uid
            && !TerminatingOrDeleted(uid)
            && HasComp<HeadRevolutionaryComponent>(uid)
            && !_mobState.IsDead(uid);
    }

    private EntityUid? FindClosestHeadRev(EntityUid uid)
    {
        var xform = Transform(uid);
        var pos = _transform.GetWorldPosition(xform);
        EntityUid? best = null;
        var bestDist = float.MaxValue;

        var query = EntityQueryEnumerator<HeadRevolutionaryComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var rev, out _, out var mobState, out var revXform))
        {
            if (_mobState.IsDead(rev, mobState) || revXform.MapID != xform.MapID)
                continue;

            var dist = (_transform.GetWorldPosition(revXform) - pos).LengthSquared();
            if (dist >= bestDist)
                continue;

            bestDist = dist;
            best = rev;
        }

        return best;
    }
}
