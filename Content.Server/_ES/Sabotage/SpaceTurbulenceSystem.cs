// SPDX-FileCopyrightText: 2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.Camera;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._ES.Sabotage;

public sealed partial class SpaceTurbulenceSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedCameraRecoilSystem _recoil = default!;

    public bool TryStart(EntityUid station)
    {
        if (TerminatingOrDeleted(station) || HasComp<SpaceTurbulenceComponent>(station)
            || _station.GetLargestGrid(station) == null)
            return false;

        var comp = AddComp<SpaceTurbulenceComponent>(station);
        comp.RemainingJerks = comp.JerkRange.Next(_random);
        comp.NextJerk = _timing.CurTime + _random.Next(comp.MinWarningTime, comp.MaxWarningTime)
            + _random.Next(comp.MinJerkDelay, comp.MaxJerkDelay);
        _chat.DispatchStationAnnouncement(station, Loc.GetString("es-sabotage-navigation-announcement"));
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<SpaceTurbulenceComponent>();
        while (query.MoveNext(out var station, out var comp))
        {
            if (comp.RemainingJerks <= 0 || _station.GetLargestGrid(station) is not {} grid)
            {
                RemCompDeferred<SpaceTurbulenceComponent>(station);
                continue;
            }
            if (_timing.CurTime < comp.NextJerk)
                continue;

            Jerk(grid, comp);
            comp.RemainingJerks--;
            if (comp.RemainingJerks == 0)
                RemCompDeferred<SpaceTurbulenceComponent>(station);
            else
                comp.NextJerk = _timing.CurTime + _random.Next(comp.MinJerkDelay, comp.MaxJerkDelay);
        }
    }

    private void Jerk(EntityUid grid, SpaceTurbulenceComponent comp)
    {
        var direction = _random.NextAngle().ToVec();
        var occupants = new List<EntityUid>();
        var query = EntityQueryEnumerator<BuckleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.GridUid == grid)
                occupants.Add(uid);
        }

        // Knockdowns can drop items and change the grid's children.
        foreach (var uid in occupants)
        {
            if (TerminatingOrDeleted(uid) || !TryComp<BuckleComponent>(uid, out var buckle))
                continue;

            _recoil.KickCamera(uid, direction * 0.8f);
            if (buckle.Buckled || Transform(uid).ParentUid != grid)
                continue;

            _stun.TryUpdateParalyzeDuration(uid, comp.StunTime);
            _throwing.TryThrow(uid, direction * comp.ThrowDistance, baseThrowSpeed: comp.ThrowForce);
        }
    }
}
