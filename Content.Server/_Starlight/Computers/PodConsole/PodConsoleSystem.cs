using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._Starlight.Computers.PodConsole;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Shuttles.Components;
using Content.Shared.Verbs;
using Robust.Shared.Timing;

namespace Content.Server._Starlight.Computers.PodConsole;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class PodConsoleSystem : SharedPodConsoleSystem
{

    [Dependency] private EmergencyShuttleSystem _emergencyShuttleSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var escapePodsQuery = EntityQueryEnumerator<PodConsoleComponent>();

        while (escapePodsQuery.MoveNext(out var ent, out var podConsole))
        {
            if (podConsole.LaunchTime == null || podConsole.LaunchTime > _timing.CurTime) continue;
            // Dumont changes start
            if (CheckOvercrowded((ent, podConsole), null))
            {
                podConsole.LaunchTime = null;
                podConsole.Locked = false;
                Dirty(ent, podConsole);
                continue;
            }
            // Dumont end
            var grid = Transform(ent).ParentUid;
            RemComp<PodConsoleComponent>(ent);
            if (!HasComp<EscapePodComponent>(grid) || !TryComp(grid, out ShuttleComponent? shuttle))
                continue;

            _emergencyShuttleSystem.LaunchEscapePod(grid, shuttle, 60f); // Dumont
        }
    }

}

