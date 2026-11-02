using Content.Shared.GameTicking;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Saae;

/// <summary>
/// Plays S.A.A.E. announcement audio one at a time.
/// Anything that comes in while another announcement is playing waits in a queue.
/// </summary>
public sealed partial class SaaeAnnouncerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    private static readonly TimeSpan Gap = TimeSpan.FromSeconds(1);

    private readonly record struct QueuedAnnouncement(ResolvedSoundSpecifier Sound, Filter Filter, AudioParams Params);

    private readonly Queue<QueuedAnnouncement> _queue = new();
    private TimeSpan _busyUntil;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_queue.Count == 0 || _timing.CurTime < _busyUntil)
            return;

        var next = _queue.Dequeue();
        Start(next.Sound, next.Filter, next.Params);
    }

    [SubscribeLocalEvent]
    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _queue.Clear();
        _busyUntil = TimeSpan.Zero;
    }

    /// <summary>
    /// Plays an announcement now if nothing else is playing, otherwise queues it.
    /// </summary>
    public void Play(SoundSpecifier? sound, Filter filter, AudioParams? audioParams = null)
    {
        if (sound == null)
            return;

        var resolved = _audio.ResolveSound(sound);
        var finalParams = audioParams ?? sound.Params;
        if (_queue.Count == 0 && _timing.CurTime >= _busyUntil)
        {
            Start(resolved, filter, finalParams);
            return;
        }

        _queue.Enqueue(new QueuedAnnouncement(resolved, filter, finalParams));
    }

    /// <summary>
    /// Plays an announcement right away, skipping the queue, and holds the queue for <paramref name="hold"/>.
    /// </summary>
    public (EntityUid Entity, AudioComponent Component)? PlayNow(SoundSpecifier sound, Filter filter, TimeSpan hold)
    {
        _busyUntil = _timing.CurTime + hold;
        return _audio.PlayGlobal(sound, filter, true);
    }

    /// <summary>
    /// Stops an announcement started with <see cref="PlayNow"/> and lets the queue continue.
    /// </summary>
    public void Stop(EntityUid? audio)
    {
        _audio.Stop(audio);
        _busyUntil = _timing.CurTime;
    }

    private void Start(ResolvedSoundSpecifier sound, Filter filter, AudioParams audioParams)
    {
        _audio.PlayGlobal(sound, filter, true, audioParams);
        _busyUntil = _timing.CurTime + _audio.GetAudioLength(sound) + Gap;
    }
}
