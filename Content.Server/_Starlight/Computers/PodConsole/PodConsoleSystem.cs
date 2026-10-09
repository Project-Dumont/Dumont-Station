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
// Dumont changes start
using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Robust.Shared.Audio.Systems;
// Dumont end
using Robust.Shared.Timing;

namespace Content.Server._Starlight.Computers.PodConsole;

/// <summary>
/// Launches escape pods when their console countdown ends.
/// </summary>
public sealed partial class PodConsoleSystem : SharedPodConsoleSystem
{

    [Dependency] private EmergencyShuttleSystem _emergencyShuttleSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    // Dumont changes start
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private NavMapSystem _navMap = default!;
    [Dependency] private RadioSystem _radio = default!;
    // Dumont end
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var escapePodsQuery = EntityQueryEnumerator<PodConsoleComponent>();

        while (escapePodsQuery.MoveNext(out var ent, out var podConsole))
        {
            // Dumont changes start
            if (podConsole.LaunchTime is { } launchTime &&
                podConsole.LaunchStream == null &&
                launchTime > _timing.CurTime &&
                launchTime - _timing.CurTime <= podConsole.LaunchSoundLead)
            {
                PlayLaunchSound((ent, podConsole));
            }
            // Dumont end

            if (podConsole.LaunchTime == null || podConsole.LaunchTime > _timing.CurTime) continue;
            // Dumont changes start
            if (CheckOvercrowded((ent, podConsole), null))
            {
                podConsole.LaunchTime = null;
                podConsole.Locked = false;
                podConsole.LaunchStream = _audio.Stop(podConsole.LaunchStream);
                Dirty(ent, podConsole);
                continue;
            }

            var countdownSound = podConsole.LaunchStream != null;
            // Dumont end
            var grid = Transform(ent).ParentUid;
            RemComp<PodConsoleComponent>(ent);
            if (!HasComp<EscapePodComponent>(grid) || !TryComp(grid, out ShuttleComponent? shuttle))
                continue;

            _emergencyShuttleSystem.LaunchEscapePod(grid, shuttle, 60f); // Dumont
            // Dumont changes start
            if (countdownSound && TryComp<FTLComponent>(grid, out var ftl))
                ftl.StartupStream = _audio.Stop(ftl.StartupStream);
            // Dumont end
        }
    }

    // Dumont changes start
    protected override void OnLaunchCountdown(Entity<PodConsoleComponent> ent)
    {
        var time = (int) ent.Comp.LaunchDelay.TotalSeconds;
        var message = _navMap.TryGetNearestBeacon(ent.Owner, out var beacon, out _) && beacon.Value.Comp.Text is { } location
            ? Loc.GetString("escape-pod-launch-radio-location", ("location", location), ("time", time))
            : Loc.GetString("escape-pod-launch-radio", ("time", time));

        _radio.SendRadioMessage(ent, message, ent.Comp.LaunchChannel, ent);
    }

    /// <summary>
    /// Plays the launch sound on the pod grid so everyone inside hears it.
    /// </summary>
    private void PlayLaunchSound(Entity<PodConsoleComponent> ent)
    {
        if (Transform(ent).GridUid is not { } grid)
            return;

        var audio = _audio.PlayPvs(ent.Comp.LaunchSound, grid);
        _audio.SetGridAudio(audio);
        ent.Comp.LaunchStream = audio?.Entity;
    }
    // Dumont end

}

