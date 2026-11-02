using Content.Server._Dumont.MalfAI;
using Content.Server._Dumont.Saae;
using Content.Server._Funkystation.MalfAI.Components;
using Content.Shared._Funkystation.CCVar;
using Content.Shared._Funkystation.MalfAI.Events;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI;

public sealed partial class MalfAiDoomsdaySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SaaeAnnouncerSystem _saae = default!;

    private static readonly SoundSpecifier SaaeHackedFirst = new SoundPathSpecifier("/Audio/_Dumont/Announcements/hacked_saae_1.ogg");
    private static readonly SoundSpecifier SaaeHackedSecond = new SoundPathSpecifier("/Audio/_Dumont/Announcements/hacked_saae_2.ogg");
    private static readonly LocId SaaeSender = "saae-announcement-sender";
    private static readonly TimeSpan SaaeHoldPadding = TimeSpan.FromSeconds(5);

    private readonly record struct SaaeStep(TimeSpan Time, LocId Message, SoundSpecifier? Sound = null, bool StartCountdown = false);

    private static readonly SaaeStep[] SaaeSteps =
    [
        new(TimeSpan.Zero, "saae-doomsday-firewall", SaaeHackedFirst),
        new(TimeSpan.FromSeconds(26), "saae-doomsday-compromised"),
        new(TimeSpan.FromSeconds(86), "saae-doomsday-reactivated", SaaeHackedSecond),
        new(TimeSpan.FromSeconds(116), "saae-doomsday-intruders"),
        new(TimeSpan.FromSeconds(143), "saae-doomsday-requested"),
        new(TimeSpan.FromSeconds(155), "saae-doomsday-accepted"),
        new(TimeSpan.FromSeconds(168), "saae-doomsday-takeover"),
        new(TimeSpan.FromSeconds(185), "saae-doomsday-minutes", StartCountdown: true),
        new(TimeSpan.FromSeconds(193), "saae-doomsday-months"),
        new(TimeSpan.FromSeconds(200), "saae-doomsday-years"),
        new(TimeSpan.FromSeconds(211), "saae-doomsday-good-luck"),
    ];

    private void StartSaaeIntro(EntityUid ai, EntityUid station, EntityUid holder)
    {
        var intro = EnsureComp<SaaeDoomsdayIntroComponent>(ai);
        intro.Station = station;
        intro.CoreHolder = holder;
        intro.StartTime = _timing.CurTime;
        intro.NextStep = 0;
        intro.Audio = null;
    }

    private void UpdateSaaeIntro()
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<SaaeDoomsdayIntroComponent>();
        while (query.MoveNext(out var uid, out var intro))
        {
            if (!HasComp<MalfAiDoomsdayComponent>(uid) && !IsInCore(uid, intro.CoreHolder))
            {
                StopSaaeIntro(uid, intro);
                AnnounceLoc(uid, "malfai-doomsday-abort-left-core", urgent: true);
                continue;
            }

            var elapsed = now - intro.StartTime;
            while (intro.NextStep < SaaeSteps.Length && elapsed >= SaaeSteps[intro.NextStep].Time)
            {
                RunSaaeStep(uid, intro, SaaeSteps[intro.NextStep]);
                intro.NextStep++;
            }

            if (intro.NextStep >= SaaeSteps.Length)
                RemCompDeferred<SaaeDoomsdayIntroComponent>(uid);
        }
    }

    private void RunSaaeStep(EntityUid ai, SaaeDoomsdayIntroComponent intro, SaaeStep step)
    {
        if (step.Sound != null)
        {
            _audio.Stop(intro.Audio);
            intro.Audio = _saae.PlayNow(step.Sound, _station.GetInOwningStation(intro.Station), GetSaaeHold(step))?.Entity;
        }

        _chat.DispatchStationAnnouncement(ai,
            Loc.GetString(step.Message),
            sender: Loc.GetString(SaaeSender),
            playDefaultSound: false,
            colorOverride: Color.Red);

        if (step.StartCountdown)
            StartSaaeCountdown(ai, intro);
    }

    private static TimeSpan GetSaaeHold(SaaeStep step)
    {
        foreach (var other in SaaeSteps)
        {
            if (other.Time > step.Time && other.Sound != null)
                return other.Time - step.Time;
        }

        return SaaeSteps[^1].Time + SaaeHoldPadding - step.Time;
    }

    private void StartSaaeCountdown(EntityUid ai, SaaeDoomsdayIntroComponent intro)
    {
        var duration = _cfg.GetCVar(CCVarsMalfAi.MalfAiDoomsdayDuration);
        var comp = EnsureComp<MalfAiDoomsdayComponent>(ai);
        InitializeDoomsdayComponent(comp, intro.Station, intro.CoreHolder, duration > 0 ? duration : 1f);

        _alerts.SetLevel(intro.Station, DoomsdayAlertLevel, playSound: false, announce: true, force: true, locked: true);
        SetupDoomsdayMusic(comp);
        RaiseLocalEvent(new MalfAiDoomsdayStartedEvent(intro.Station, ai));
    }

    private bool IsInCore(EntityUid ai, EntityUid holder)
    {
        return HasComp<StationAiHeldComponent>(ai) &&
            _containers.TryGetContainingContainer((ai, null, null), out var container) &&
            container.Owner == holder &&
            HasComp<StationAiCoreComponent>(holder);
    }

    private void StopSaaeIntro(EntityUid ai, SaaeDoomsdayIntroComponent? intro = null)
    {
        if (!Resolve(ai, ref intro, false))
            return;

        _saae.Stop(intro.Audio);
        RemCompDeferred<SaaeDoomsdayIntroComponent>(ai);
    }
}
