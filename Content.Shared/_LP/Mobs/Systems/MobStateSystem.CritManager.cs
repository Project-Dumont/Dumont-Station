using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Player;
using Content.Shared.Damage.Components;
using Robust.Shared.Audio.Systems;
using AudioComponent = Robust.Shared.Audio.Components.AudioComponent;
using Content.Shared.Movement.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mind;

namespace Content.Shared.Mobs.Systems;

public partial class MobStateSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    private readonly Dictionary<EntityUid, EntityUid> _stateAudio = new();

    private static readonly Dictionary<MobState, (string sound, bool loop, float volume)> StateAudio
        = new()
    {
        { MobState.SoftCritical, ("/Audio/_LP/Effects/soft_critical.ogg", true, -6f) },
        { MobState.HardCritical, ("/Audio/_LP/Effects/critical.ogg", true, -8f) },
        { MobState.Alive, ("/Audio/_LP/Effects/backtolife.ogg", false, -4f) },
    };

    /* private void OnUpdateCanMove(EntityUid uid, MobStateComponent component, ref UpdateCanMoveEvent args)
    {
        if (component.CurrentState == MobState.SoftCritical)
        {
            if (TryComp<DamageableComponent>(uid, out var damage))
            {
                var total = _damageable.GetTotalDamage((uid, damage));

                if (total > 150)
                {
                    args.Cancel();
                    return;
                }
            }
            return;
        }

        CheckAct(uid, component, args);
    }

    private void OnSoftCritSpeed(
    EntityUid uid,
    MobStateComponent component,
    ref RefreshMovementSpeedModifiersEvent args)
    {
        if (component.CurrentState != MobState.SoftCritical)
            return;

        args.ModifySpeed(0.35f, 0.35f);
    } */

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
