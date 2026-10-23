using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._Dumont.EscapePods;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.EscapePods;

/// <summary>
/// Launches escape pods when their console countdown ends.
/// </summary>
public sealed partial class PodConsoleSystem : SharedPodConsoleSystem
{
    [Dependency] private EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private NavMapSystem _navMap = default!;
    [Dependency] private RadioSystem _radio = default!;

    /// <summary>
    /// How long the trip from the station to the evacuation planet takes.
    /// </summary>
    private const float TravelTime = 60f;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PodConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            if (console.LaunchTime is not { } launchTime)
                continue;

            if (launchTime > _timing.CurTime)
            {
                if (console.LaunchStream == null && launchTime - _timing.CurTime <= console.LaunchSoundLead)
                    PlayLaunchSound((uid, console));

                continue;
            }

            if (CheckOvercrowded((uid, console), null))
            {
                console.LaunchTime = null;
                console.Locked = false;
                console.LaunchStream = _audio.Stop(console.LaunchStream);
                Dirty(uid, console);
                continue;
            }

            var countdownSound = console.LaunchStream != null;
            var grid = Transform(uid).ParentUid;
            RemComp<PodConsoleComponent>(uid);
            if (!HasComp<EscapePodComponent>(grid) || !TryComp(grid, out ShuttleComponent? shuttle))
                continue;

            _emergencyShuttle.LaunchEscapePod(grid, shuttle, TravelTime);

            if (countdownSound && TryComp<FTLComponent>(grid, out var ftl))
                ftl.StartupStream = _audio.Stop(ftl.StartupStream);
        }
    }

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
}
