using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Chat;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public enum XenomorphNeuroHallucination
{
    AlienAttack,
    Bombardment,
    Screech,
    Airstrike,
    Mortar,
    Giggle,
    Sounds,
}

[RegisterComponent]
public sealed partial class XenomorphNeuroHallucinationsComponent : Component
{
    public readonly List<XenomorphNeuroHallucinationSequence> Sequences = new();
    public readonly List<EntityUid> Audio = new();
}

public sealed partial class XenomorphNeuroHallucinationSequence
{
    public XenomorphNeuroHallucination Kind;
    public int Stage;
    public float Delay;
    public EntityCoordinates Position;
}

public sealed partial class XenomorphNeuroHallucinationSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ISharedPlayerManager _players = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private StatusEffectsSystem _status = default!;

    private static readonly (XenomorphNeuroHallucination Kind, float Weight)[] Hallucinations =
    {
        (XenomorphNeuroHallucination.AlienAttack, 0.05f),
        (XenomorphNeuroHallucination.Bombardment, 0.05f),
        (XenomorphNeuroHallucination.Screech, 0.06f),
        (XenomorphNeuroHallucination.Airstrike, 0.08f),
        (XenomorphNeuroHallucination.Mortar, 0.18f),
        (XenomorphNeuroHallucination.Giggle, 0.27f),
        (XenomorphNeuroHallucination.Sounds, 0.31f),
    };

    private static readonly SoundSpecifier[] RandomSounds =
    {
        new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_distantroar_3.ogg"),
        new SoundPathSpecifier("/Audio/_RMC14/Xeno/xenos_roaring.ogg"),
        new SoundCollectionSpecifier("DumontXenomorphQueenBreath"),
        new SoundCollectionSpecifier("DumontXenomorphRoar"),
        new SoundPathSpecifier("/Audio/_RMC14/Announcements/Marine/notice2.ogg"),
        new SoundPathSpecifier("/Audio/_RMC14/Weapons/alien_knockdown.ogg"),
        new SoundPathSpecifier("/Audio/_RMC14/Weapons/Guns/Gunshots/gun_m54c_1.ogg"),
        new SoundCollectionSpecifier("MetalThud"),
        new SoundPathSpecifier("/Audio/Items/crowbar.ogg"),
        new SoundCollectionSpecifier("WindowShatter"),
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphNeurotoxinComponent, ComponentRemove>(OnNeuroRemoved);
        SubscribeLocalEvent<XenomorphNeuroHallucinationsComponent, ComponentShutdown>(OnShutdown);
    }

    private EntityCoordinates Offset(EntityCoordinates coordinates, float distance) => coordinates.Offset(new Vector2(
        _random.NextFloat(-distance, distance + 0.01f), _random.NextFloat(-distance, distance + 0.01f)));

    private void Sound(EntityUid victim, SoundSpecifier sound, EntityCoordinates coordinates)
    {
        if (TerminatingOrDeleted(victim) || TerminatingOrDeleted(coordinates.EntityId)) return;
        if (_audio.PlayStatic(sound, Filter.Entities(victim), coordinates, false) is not { } audio) return;
        EnsureComp<XenomorphNeuroHallucinationsComponent>(victim).Audio.Add(audio.Entity);
    }

    private void Sound(EntityUid victim, string path, EntityCoordinates coordinates, float volume = 0) =>
        Sound(victim, new SoundPathSpecifier(path, AudioParams.Default.WithVolume(volume)), coordinates);

    private void Warning(EntityUid victim, string message)
    {
        _popup.PopupEntity(message, victim, victim, PopupType.LargeCaution);
        if (!_players.TryGetSessionByEntity(victim, out var session)) return;
        var wrapped = $"[bold][font size=24][color=red]{message}[/color][/font][/bold]";
        _chatManager.ChatMessageToOne(ChatChannel.Radio, message, wrapped, default, false, session.Channel, recordReplay: false);
    }

    private void DirectionWarning(EntityUid victim, EntityCoordinates position, bool mortar)
    {
        var delta = _transform.ToMapCoordinates(position).Position - _transform.GetMapCoordinates(victim).Position;
        var key = mortar ? "xenomorph-hallucination-mortar" : "xenomorph-hallucination-airstrike";
        var message = delta.LengthSquared() < 1 ? Loc.GetString(key + "-above")
            : Loc.GetString(key, ("direction", Loc.GetString("xenomorph-hallucination-direction-" + delta.GetDir().ToString().ToLowerInvariant())));
        Warning(victim, message);
    }

    private bool Living(EntityUid victim) => HasComp<MobStateComponent>(victim) && !_mobs.IsDead(victim);

    public bool TryHallucinate(EntityUid victim, XenomorphNeuroHallucination kind)
    {
        if (TerminatingOrDeleted(victim) || !Living(victim)
            || !TryComp<XenomorphNeurotoxinComponent>(victim, out var neuro) || neuro.Amount < 15
            || _transform.GetMapCoordinates(victim).MapId == MapId.Nullspace) return false;
        var coordinates = _transform.GetMoverCoordinates(victim);
        var position = Offset(coordinates, kind == XenomorphNeuroHallucination.Screech ? 3 : 7);
        var sequence = new XenomorphNeuroHallucinationSequence { Kind = kind, Position = position };
        switch (kind)
        {
            case XenomorphNeuroHallucination.AlienAttack:
                Sound(victim, "/Audio/_RMC14/Xeno/alien_pounce.ogg", coordinates);
                _stun.TryUpdateParalyzeDuration(victim, TimeSpan.FromSeconds(5));
                sequence.Delay = 1;
                break;
            case XenomorphNeuroHallucination.Bombardment:
                Warning(victim, Loc.GetString("xenomorph-hallucination-bombardment"));
                Sound(victim, "/Audio/_RMC14/Effects/ob_alert.ogg", coordinates);
                sequence.Delay = 2;
                break;
            case XenomorphNeuroHallucination.Screech:
                Sound(victim, "/Audio/_RMC14/Xeno/alien_queen_screech.ogg", position, -7);
                _stun.TryUpdateParalyzeDuration(victim, TimeSpan.FromSeconds(2));
                return true;
            case XenomorphNeuroHallucination.Airstrike:
                Sound(victim, "/Audio/_RMC14/Weapons/dropship_sonic_boom.ogg", position);
                sequence.Delay = 3.5f;
                break;
            case XenomorphNeuroHallucination.Mortar:
                DirectionWarning(victim, position, true);
                sequence.Delay = 1;
                break;
            case XenomorphNeuroHallucination.Giggle:
                _chat.TryEmoteWithChat(victim, "Laugh", ignoreActionBlocker: true, forceEmote: true);
                _status.TryUpdateStatusEffectDuration(victim, "StatusEffectSeeingRainbow", TimeSpan.FromSeconds(6));
                return true;
            case XenomorphNeuroHallucination.Sounds:
                Sound(victim, _random.Pick(RandomSounds), position);
                return true;
            default: return false;
        }
        EnsureComp<XenomorphNeuroHallucinationsComponent>(victim).Sequences.Add(sequence);
        return true;
    }

    private bool Advance(EntityUid victim, XenomorphNeuroHallucinationSequence sequence)
    {
        var coordinates = _transform.GetMoverCoordinates(victim);
        switch (sequence.Kind)
        {
            case XenomorphNeuroHallucination.AlienAttack:
                if (sequence.Stage < 3) Sound(victim, new SoundCollectionSpecifier("AlienClaw"), coordinates);
                if (sequence.Stage is 0 or 3) Sound(victim, "/Audio/_RMC14/Weapons/alien_knockdown.ogg", coordinates);
                if (sequence.Stage == 3)
                {
                    _chat.TryEmoteWithChat(victim, "Scream", ignoreActionBlocker: true, forceEmote: true);
                    return true;
                }
                sequence.Stage++;
                sequence.Delay = 0.5f;
                return false;
            case XenomorphNeuroHallucination.Bombardment:
                Sound(victim, "/Audio/_RMC14/Weapons/gun_orbital_travel.ogg", Offset(coordinates, 7));
                return true;
            case XenomorphNeuroHallucination.Mortar:
                Sound(victim, "/Audio/_RMC14/Weapons/gun_mortar_travel.ogg", sequence.Position);
                return true;
            case XenomorphNeuroHallucination.Airstrike:
                AdvanceAirstrike(victim, sequence);
                if (sequence.Stage++ < 11) return false;
                _chat.TryEmoteWithChat(victim, "Scream", ignoreActionBlocker: true, forceEmote: true);
                return true;
        }
        return true;
    }

    private void AdvanceAirstrike(EntityUid victim, XenomorphNeuroHallucinationSequence sequence)
    {
        var position = sequence.Position;
        var stage = sequence.Stage;
        if (stage == 0) DirectionWarning(victim, position, false);
        if (stage is 1 or 3 or 5 or 8) Sound(victim, "/Audio/_RMC14/Effects/rocketpod_fire.ogg", Offset(position, 7));
        if (stage == 2) Sound(victim, "/Audio/_RMC14/Dropship/gau.ogg", Offset(position, 7));
        if (stage is 3 or 4 or 5 or 6 or 7 or 9 or 10 or 11)
            Sound(victim, "/Audio/_RMC14/Dropship/gauimpact.ogg", Offset(position, 7), -5);
        if (stage is 3 or 6) Sound(victim, "/Audio/_RMC14/Dropship/gauimpact.ogg", Offset(position, 7), -5);
        if (stage is 4 or 6 or 9 or 11)
            Sound(victim, new SoundCollectionSpecifier("DumontXenomorphExplosion"), Offset(position, 7));
        if (stage == 7) Sound(victim, new SoundCollectionSpecifier("DumontXenomorphBigExplosion"), Offset(position, 7));
        sequence.Delay = stage is 3 or 4 or 5 ? 1 : 0.5f;
    }

    private void OnNeuroRemoved(Entity<XenomorphNeurotoxinComponent> ent, ref ComponentRemove args) =>
        RemCompDeferred<XenomorphNeuroHallucinationsComponent>(ent);

    private void OnShutdown(Entity<XenomorphNeuroHallucinationsComponent> ent, ref ComponentShutdown args)
    {
        foreach (var audio in ent.Comp.Audio) if (Exists(audio)) _audio.Stop(audio);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphNeurotoxinComponent>();
        while (query.MoveNext(out var uid, out var neuro))
        {
            if (neuro.Amount < 15 || !Living(uid) || _timing.CurTime < neuro.NextHallucination) continue;
            neuro.NextHallucination = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(4, 11));
            var choice = _random.NextFloat();
            foreach (var (kind, weight) in Hallucinations)
            {
                choice -= weight;
                if (choice > 0) continue;
                TryHallucinate(uid, kind);
                break;
            }
        }
        var lingering = EntityQueryEnumerator<XenomorphNeuroHallucinationsComponent>();
        while (lingering.MoveNext(out var uid, out var comp))
        {
            if (!HasComp<XenomorphNeurotoxinComponent>(uid) || !Living(uid))
            {
                RemCompDeferred<XenomorphNeuroHallucinationsComponent>(uid);
                continue;
            }
            for (var i = comp.Sequences.Count - 1; i >= 0; i--)
            {
                var sequence = comp.Sequences[i];
                sequence.Delay -= frameTime;
                if (sequence.Delay <= 0 && Advance(uid, sequence)) comp.Sequences.RemoveAt(i);
            }
            comp.Audio.RemoveAll(audioUid => !Exists(audioUid));
            if (comp.Sequences.Count == 0 && comp.Audio.Count == 0) RemCompDeferred<XenomorphNeuroHallucinationsComponent>(uid);
        }
    }
}
