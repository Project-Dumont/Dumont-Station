using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Prayer;
using Content.Shared.Actions;
using Content.Shared.Database;
using Content.Shared.Genetics;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server.MindCommunication;

public sealed class MindCommunicationGenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly IAdminLogManager _admin = default!;
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;
    [Dependency] private readonly PrayerSystem _prayerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindCommunicationGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MindCommunicationGenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MindCommunicationGenComponent, MindCommunicationActionEvent>(OnMindCommunication);

        SubscribeNetworkEvent<MindCommunicationTargetSelectedEvent>(OnTargetSelected);
    }

    private void OnInit(Entity<MindCommunicationGenComponent> ent, ref ComponentInit args)
        => ent.Comp.ActionEntity = _action.AddAction(ent, ent.Comp.Action);

    private void OnShutdown(Entity<MindCommunicationGenComponent> ent, ref ComponentShutdown args)
        => _action.RemoveAction(ent.Comp.ActionEntity);

    private void OnMindCommunication(Entity<MindCommunicationGenComponent> ent, ref MindCommunicationActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        RaiseNetworkEvent(new MindCommunicationMenuOpenedEvent(GetNetEntity(ent)));
    }

    private void OnTargetSelected(MindCommunicationTargetSelectedEvent args)
    {
        var sender = GetEntity(args.Sender);
        var target = GetEntity(args.Target);

        if (!TryComp<ActorComponent>(sender, out var senderActor) ||
            !TryComp<ActorComponent>(target, out var targetActor))
            return;

        if (HasComp<PsyResistGenComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("mind-communication-blocked", ("name", Name(target))), sender, sender);
            return;
        }

        _quickDialog.OpenDialog(senderActor.PlayerSession,
            Loc.GetString("mind-communication-dialog-title"), "",
            (string message) =>
            {
                if (string.IsNullOrWhiteSpace(message))
                    return;

                var popupMessage = Loc.GetString("mind-communication-message", ("message", message));
                _prayerSystem.SendSubtleMessage(targetActor.PlayerSession, targetActor.PlayerSession, string.Empty, popupMessage);

                _admin.Add(LogType.Chat, LogImpact.Low,
                    $"{ToPrettyString(sender):user} sent mind message to {ToPrettyString(target):target}: {message}");
            });
    }
}
