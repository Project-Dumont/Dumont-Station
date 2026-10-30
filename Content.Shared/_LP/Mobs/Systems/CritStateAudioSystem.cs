using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Configuration;
using Content.Shared._LP.CCVar;
using Content.Shared._LP.Mobs.Events;

namespace Content.Shared._LP.Mobs.Systems;

public sealed partial class CritStateAudioSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetConfigurationManager _configuration = default!;

    private readonly Dictionary<EntityUid, EntityUid> _stateAudio = new();

    private static readonly Dictionary<MobState, (string sound, bool loop, float volume)> StateAudio = new()
    {
        { MobState.Alive, ("/Audio/_LP/Effects/backtolife.ogg", false, -4f) },
        { MobState.SoftCritical, ("/Audio/_LP/Effects/soft_critical.ogg", true, -6f) },
        { MobState.HardCritical, ("/Audio/_LP/Effects/critical.ogg", true, -8f) },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MobStateComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeNetworkEvent<CritStateAudioEvent>(ToggleCriticalSound);
    }

    private void OnMobStateChanged(Entity<MobStateComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.OldMobState == args.NewMobState)
            return;

        StopStateAudio(ent.Owner);

        if (StateAudio.TryGetValue(args.NewMobState, out var data))
            PlayStateAudio(ent.Owner, data.sound, data.loop, data.volume);
    }

    private void OnComponentShutdown(Entity<MobStateComponent> ent, ref ComponentShutdown args)
    {
        StopStateAudio(ent.Owner);
    }

    private void ToggleCriticalSound(CritStateAudioEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } uid || args.SenderSession.Channel is not { } channel)
            return;

        StopStateAudio(uid);

        if (_configuration.GetClientCVar(channel, LPCVars.CriticalSoundEnabled) && TryComp<MobStateComponent>(uid, out var mob) && mob.CurrentState is MobState.SoftCritical or MobState.HardCritical && StateAudio.TryGetValue(mob.CurrentState, out var data))
            PlayStateAudio(uid, data.sound, data.loop, data.volume);
    }

    private void PlayStateAudio(EntityUid uid, string sound, bool loop, float volume)
    {
        if (!_timing.IsFirstTimePredicted || !TryComp<ActorComponent>(uid, out var actor) || actor.PlayerSession.Channel is not { } channel || !_configuration.GetClientCVar(channel, LPCVars.CriticalSoundEnabled))
            return;

        StopStateAudio(uid);

        var audio = _audio.PlayEntity(new SoundPathSpecifier(sound), actor.PlayerSession, uid, new AudioParams { Loop = loop, Volume = volume });

        if (audio == null)
            return;

        _stateAudio[uid] = audio.Value.Entity;
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
