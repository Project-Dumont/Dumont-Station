using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared._LP.Mobs.Events;

namespace Content.Shared._LP.Mobs.Systems;

public sealed partial class CatchBreathSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;

    private readonly Dictionary<EntityUid, EntityUid> _tryAudio = new();

    private const float DoAfterTime = 6f;
    private const float BluntSuccessChance = 0.03f;
    private const float AsphyxiaSuccessChance = 0.63f;
    private const float FailureChance = 0.78f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CatchBreathAlertEvent>(OnAlertClicked);
        SubscribeLocalEvent<CatchBreathDoAfterEvent>(OnDoAfter);
    }

    private void OnAlertClicked(CatchBreathAlertEvent ev)
    {
        if (!_net.IsServer)
            return;

        var uid = ev.User;

        if (!TryComp<MobStateComponent>(uid, out var mob) || mob.CurrentState != MobState.SoftCritical)
            return;

        var args = new DoAfterArgs(EntityManager, uid, DoAfterTime, new CatchBreathDoAfterEvent(), uid)
        {
            Broadcast = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
            RequireCanInteract = false,
        };

        if (!_doAfter.TryStartDoAfter(args))
            return;

        _popup.PopupEntity(Loc.GetString("catch-breath-try"), uid, uid);

        StopTryAudio(uid);

        var audio = _audio.PlayEntity(new SoundPathSpecifier("/Audio/_LP/Alerts/Event/CatchBreath/catch-breath-try.ogg"), uid, uid);

        if (audio != null)
            _tryAudio[uid] = audio.Value.Entity;

        _adminLogger.Add(LogType.CatchBreath, LogImpact.Low, $"{ev.User} start trying catch breath");
    }

    private void OnDoAfter(CatchBreathDoAfterEvent ev)
    {
        if (!_net.IsServer)
            return;

        var uid = ev.User;

        if (ev.Cancelled)
        {
            StopTryAudio(uid);
            return;
        }

        if (!TryComp<MobStateComponent>(uid, out var mob) || mob.CurrentState != MobState.SoftCritical)
        {
            StopTryAudio(uid);
            return;
        }

        var roll = _random.NextFloat();

        _adminLogger.Add(LogType.CatchBreath, LogImpact.Low, $"{ev.User} got roll result {roll}");

        var damage = new DamageSpecifier();
        var audio = new SoundPathSpecifier("");

        if (roll < BluntSuccessChance)
        {
            damage.DamageDict.Add("Blunt", -2);
            damage.DamageDict.Add("Slash", -2);
            damage.DamageDict.Add("Piercing", -2);
            damage.DamageDict.Add("Asphyxiation", -10);
            _adminLogger.Add(LogType.CatchBreath, LogImpact.Low, $"{ev.User} has got BLUNT SUCCESS!");
            _popup.PopupEntity(Loc.GetString("catch-breath-blunt-success"), uid, uid);
            audio = new SoundPathSpecifier("/Audio/_LP/Alerts/Event/CatchBreath/catch-breath-bluntsuccess.ogg");
        }
        else if (roll < AsphyxiaSuccessChance)
        {
            damage.DamageDict.Add("Asphyxiation", -7);
            _adminLogger.Add(LogType.CatchBreath, LogImpact.Low, $"{ev.User} has got SUCCESS!");
            _popup.PopupEntity(Loc.GetString("catch-breath-success"), uid, uid);
            audio = new SoundPathSpecifier("/Audio/_LP/Alerts/Event/CatchBreath/catch-breath-success.ogg");
        }
        else if (roll < FailureChance)
        {
            damage.DamageDict.Add("Asphyxiation", 5);
            _adminLogger.Add(LogType.CatchBreath, LogImpact.Low, $"{ev.User} has got FAILURE!");
            _popup.PopupEntity(Loc.GetString("catch-breath-failure"), uid, uid);
            audio = new SoundPathSpecifier("/Audio/_LP/Alerts/Event/CatchBreath/catch-breath-failure.ogg");
        }
        else
        {
            _adminLogger.Add(LogType.CatchBreath, LogImpact.Low, $"{ev.User} has got NOTHING!");
            _popup.PopupEntity(Loc.GetString("catch-breath-nothing"), uid, uid);
            audio = new SoundPathSpecifier("/Audio/_LP/Alerts/Event/CatchBreath/catch-breath-nothing.ogg");
        }

        StopTryAudio(uid);
        _audio.PlayEntity(audio, uid, uid);
        _damage.TryChangeDamage(uid, damage);
    }

    private void StopTryAudio(EntityUid uid)
    {
        if (!_tryAudio.TryGetValue(uid, out var audio))
            return;

        _audio.Stop(audio);
        _tryAudio.Remove(uid);
    }
}
