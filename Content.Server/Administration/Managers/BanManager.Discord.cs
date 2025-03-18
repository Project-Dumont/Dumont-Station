using Content.Server.Discord;
using Content.Server.GameTicking;
using Content.Shared._Gabystation.CCVar;
using Robust.Shared.Network;
using Content.Server.Database;

namespace Content.Server.Administration.Managers;

// Gabystation - Serverban & Roleban webhook
public sealed partial class BanManager
{
    [Dependency] private readonly DiscordWebhook _discord = default!;
    private WebhookData? _webhook;

    private void InitializeDiscord()
    {
        var value = _cfg.GetCVar(GabyCVars.BanDiscordWebhook);
        if (!string.IsNullOrEmpty(value))
        {
            _discord.TryGetWebhook(value, val => _webhook = val);
        }
    }
    // TODO: This is shitcode, revamp.
    // TODO: Embed.

    /// <summary>
    /// Send to webhook a server ban message.
    /// </summary>
    private async void SendServerBanWebhook(ServerBanDef ban, string player, string admin, uint? minutes)
    {
        try
        {
            if (_webhook is null)
                return;

            var hook = _webhook.Value.ToIdentifier();

            var id = ban.Id?.ToString() ?? "?";
            var rId = ban.RoundId?.ToString() ?? "?";



            var footer = Loc.GetString("ban-manager-notify-discord-footer",
                ("round", rId),
                ("id", id)); // Todo: fallback

            var message = "...";

            if (ban.ExpirationTime is null)
                message = Loc.GetString("ban-manager-notify-discord-perma",
                ("admin", admin),
                ("player", player),
                ("reason", ban.Reason));
            else
                message = Loc.GetString("ban-manager-notify-discord",
                ("admin", admin),
                ("player", player),
                ("time", FormatTime(minutes)),
                ("reason", ban.Reason));

            var payload = new WebhookPayload
            {
                Content = $"{message}\n{footer}",
            };

            await _discord.CreateMessage(hook, payload);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to send ban information to webhook!\n{e}");
        }
    }

    /// <summary>
    /// Send to webhook a role ban message.
    /// </summary>
    private async void SendRoleBanWebhook(ServerRoleBanDef ban, string player, string admin, uint? minutes)
    { //roleban is SHIT, it send a message to every role
        try
        {
            if (_webhook is null)
                return;

            var hook = _webhook.Value.ToIdentifier();

            var id = ban.Id?.ToString() ?? "?";
            var rId = ban.RoundId?.ToString() ?? "?";

            var footer = Loc.GetString("ban-manager-notify-discord-footer",
                ("round", rId),
                ("id", id));

            var message = "...";

            if (ban.ExpirationTime is null)
                message = Loc.GetString("ban-manager-notify-discord-role-perma",
                ("admin", admin),
                ("role", ban.Role),
                ("player", player),
                ("reason", ban.Reason));
            else
                message = Loc.GetString("ban-manager-notify-discord-role",
                ("admin", admin),
                ("player", player),
                ("role", ban.Role),
                ("time", FormatTime(minutes)),
                ("reason", ban.Reason));

            var payload = new WebhookPayload
            {
                Content = $"{message}\n{footer}",
            };

            await _discord.CreateMessage(hook, payload);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to send roleban information to webhook!\n{e}");
        }
    }

    private string FormatTime(uint? time)
    {
        if (time is null)
            return Loc.GetString("ban-manager-notify-discord-permanent");

        var minutes = time ?? 0;

        if (minutes < 60)
            return Loc.GetString("ban-manager-notify-discord-format-minutes", ("minutes", minutes));
        
        if (minutes < 1440)
            return Loc.GetString("ban-manager-notify-discord-format-hours", ("hours", minutes / 60));
        
        return Loc.GetString("ban-manager-notify-discord-format-days", ("days", minutes / 1440));
    }
}
