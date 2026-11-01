using Content.Shared._Dumont.EscapePods;
using Content.Shared.Access;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Communications;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.PDA;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Communications;

/// <summary>
/// Lets the captain unlock every escape pod console from a communications console, once per round.
/// </summary>
public sealed partial class CommunicationsConsoleSystem
{
    private static readonly ProtoId<AccessLevelPrototype> CaptainAccess = "Captain";

    /// <summary>
    /// Whether the escape pods were unlocked this round.
    /// </summary>
    public bool EscapePodsUnlocked { get; private set; }

    private void InitializeDumont()
    {
        SubscribeLocalEvent<CommunicationsConsoleComponent, CommunicationsConsoleUnlockEscapePodsMessage>(OnUnlockEscapePodsMessage);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => EscapePodsUnlocked = false);
    }

    private bool IsCaptain(EntityUid user)
    {
        return _accessReaderSystem.FindAccessTags(user).Contains(CaptainAccess);
    }

    private void OnUnlockEscapePodsMessage(Entity<CommunicationsConsoleComponent> ent, ref CommunicationsConsoleUnlockEscapePodsMessage args)
    {
        if (EscapePodsUnlocked || !ent.Comp.CanShuttle)
            return;

        var user = args.Actor;
        if (!TryComp<ActorComponent>(user, out var actor))
            return;

        if (!IsCaptain(user))
        {
            _popupSystem.PopupEntity(Loc.GetString("comms-console-permission-denied"), ent, user);
            return;
        }

        _quickDialog.OpenDialog(actor.PlayerSession,
            Loc.GetString("comms-console-menu-dialog-escape-pods-title"),
            Loc.GetString("comms-console-menu-dialog-escape-pods-message"),
            (string reason) =>
            {
                if (EscapePodsUnlocked || Deleted(ent) || !ent.Comp.CanShuttle || !IsCaptain(user))
                    return;

                reason = SharedChatSystem.SanitizeAnnouncement(reason, _cfg.GetCVar(CCVars.ChatMaxAnnouncementLength));
                if (string.IsNullOrWhiteSpace(reason))
                {
                    _popupSystem.PopupEntity(Loc.GetString("comms-console-empty-input"), ent, user);
                    return;
                }

                SetEscapePodsLocked(false);

                var message = Loc.GetString("comms-console-escape-pods-announcement", ("reason", reason));
                Loc.TryGetString(ent.Comp.Title, out var title);
                _chatSystem.DispatchStationAnnouncement(ent, message, title ?? ent.Comp.Title, colorOverride: Color.Red);

                var pdaEv = new PdaNotificationEvent(Loc.GetString("escape-pods-pda-notification"), "StationAlerts", true, _stationSystem.GetOwningStation(ent));
                RaiseLocalEvent(pdaEv);

                _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(user):player} unlocked the escape pods with reason '{reason}'.");
            });
    }

    /// <summary>
    /// Locks or unlocks every escape pod console and refreshes the communications consoles.
    /// </summary>
    public void SetEscapePodsLocked(bool locked)
    {
        EscapePodsUnlocked = !locked;

        var query = EntityQueryEnumerator<PodConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            console.Locked = locked;
            Dirty(uid, console);
        }

        UpdateCommsConsoleInterface();
    }
}
