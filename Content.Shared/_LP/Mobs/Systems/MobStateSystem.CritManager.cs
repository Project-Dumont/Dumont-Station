using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Mobs.Systems;

public partial class MobStateSystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    private readonly Dictionary<EntityUid, EntityUid> _stateAudio = new();

    private static readonly Dictionary<MobState, (string sound, bool loop, float volume)> StateAudio
        = new()
    {
        { MobState.SoftCritical, ("/Audio/_LP/Effects/soft_critical.ogg", true, -6f) },
        { MobState.HardCritical, ("/Audio/_LP/Effects/critical.ogg", true, -8f) },
        { MobState.Alive, ("/Audio/_LP/Effects/backtolife.ogg", false, -4f) },
    };

    private void PlayStateAudio(EntityUid uid, MobState state)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!StateAudio.TryGetValue(state, out var data))
            return;

        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        StopStateAudio(uid);

        var spec = new SoundPathSpecifier(data.sound);

        var audioParams = new AudioParams
        {
            Loop = data.loop,
            Volume = data.volume
        };

        var audio = _audio.PlayEntity(
            spec,
            Filter.SinglePlayer(actor.PlayerSession),
            uid,
            false,
            audioParams);

        if (audio == null)
        {
            _sawmill.Error("Audio NULL");
            return;
        }

        _stateAudio[uid] = audio.Value.Entity;

        _sawmill.Info($"Audio started {audio.Value.Entity}");
    }

    private void StopStateAudio(EntityUid uid)
    {
        if (!_stateAudio.TryGetValue(uid, out var audio))
            return;

        if (Exists(audio))
            QueueDel(audio);

        _stateAudio.Remove(uid);
    }
}
