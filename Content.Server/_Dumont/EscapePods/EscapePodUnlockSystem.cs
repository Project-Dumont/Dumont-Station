using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Server.Popups;
using Content.Shared._Starlight.Computers.PodConsole;
using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Communications;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.EscapePods;

public sealed partial class EscapePodUnlockSystem : EntitySystem
{
    [Dependency] private AccessReaderSystem _accessReader = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private CommunicationsConsoleSystem _comms = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private QuickDialogSystem _quickDialog = default!;

    private static readonly ProtoId<AccessLevelPrototype> CaptainAccess = "Captain";

    public bool Unlocked { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleUnlockEscapePodsMessage>(OnUnlockMessage);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => Unlocked = false);
    }

    private void OnUnlockMessage(Entity<CommunicationsConsoleComponent> ent, ref CommunicationsConsoleUnlockEscapePodsMessage args)
    {
        if (Unlocked || !ent.Comp.CanShuttle)
            return;

        var user = args.Actor;
        if (!TryComp<ActorComponent>(user, out var actor))
            return;

        if (!_accessReader.FindAccessTags(user).Contains(CaptainAccess))
        {
            _popup.PopupEntity(Loc.GetString("comms-console-permission-denied"), ent, user);
            return;
        }

        _quickDialog.OpenDialog(actor.PlayerSession,
            Loc.GetString("comms-console-menu-dialog-escape-pods-title"),
            Loc.GetString("comms-console-menu-dialog-escape-pods-message"),
            (string reason) =>
            {
                if (Unlocked || Deleted(ent))
                    return;

                reason = SharedChatSystem.SanitizeAnnouncement(reason, _cfg.GetCVar(CCVars.ChatMaxAnnouncementLength));
                if (string.IsNullOrWhiteSpace(reason))
                {
                    _popup.PopupEntity(Loc.GetString("comms-console-empty-input"), ent, user);
                    return;
                }

                UnlockEscapePods();

                var message = Loc.GetString("comms-console-escape-pods-announcement", ("reason", reason));
                Loc.TryGetString(ent.Comp.Title, out var title);
                _chat.DispatchStationAnnouncement(ent, message, title ?? ent.Comp.Title, colorOverride: Color.Red);

                _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(user):player} unlocked the escape pods with reason '{reason}'.");
            });
    }

    public void UnlockEscapePods()
    {
        Unlocked = true;

        var query = EntityQueryEnumerator<PodConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            console.Locked = false;
            Dirty(uid, console);
        }

        _comms.UpdateCommsConsoleInterface();
    }
}
