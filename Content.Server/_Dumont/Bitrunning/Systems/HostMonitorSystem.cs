using Content.Shared._Orion.Bitrunning.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Trigger;

namespace Content.Server._Orion.Bitrunning.Systems;

public sealed class HostMonitorSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HostMonitorComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(EntityUid uid, HostMonitorComponent component, ref TriggerEvent args)
    {
        if (args.User is not { } user)
            return;

        if (TryComp<AvatarConnectionComponent>(user, out var avatar) && avatar.OriginalBody is { } hostBody && Exists(hostBody) && TryComp<DamageableComponent>(hostBody, out var damageable) && _mobThreshold.TryGetDeadThreshold(hostBody, out var deadThreshold) && deadThreshold.Value > 0)
        {
            var percentage = Math.Clamp((int) ((1f - _mobThreshold.CheckVitalDamage(hostBody, damageable).Float() / deadThreshold.Value.Float()) * 100), 0, 100);
            var message = Loc.GetString("host-monitor-health", ("percentage", percentage));
            _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, hideChat: true);
        }
        else
        {
            var message = Loc.GetString("host-monitor-error");
            _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, hideChat: true);
        }

        args.Handled = true;
    }
}
