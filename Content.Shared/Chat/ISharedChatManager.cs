// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network; // Dumont

namespace Content.Shared.Chat;

public interface ISharedChatManager
{
    void Initialize();
    void SendAdminAlert(string message);
    void SendAdminAlert(EntityUid player, string message);

    /// <summary>
    /// This is a dangerous function! Only pass in property escaped text.
    /// See: <see cref="SendAdminAlert(string)"/>
    /// <br/><br/>
    /// Use this for things that need to be unformatted (like tpto links) but ensure that everything else
    /// is formated properly. If it's not, players could sneak in ban links or other nasty commands that the admins
    /// could clink on.
    /// </summary>
    void SendAdminAlertNoFormatOrEscape(string message);

    /// <summary>
    /// Trauma - moved from server, sends a message to a single player.
    /// </summary>
    void ChatMessageToOne(ChatChannel channel, string message, string wrappedMessage, EntityUid source, bool hideChat,
        INetChannel client, Color? colorOverride = null, bool recordReplay = false, string? audioPath = null, float audioVolume = 0, NetUserId? author = null, bool canCoalesce = true, bool hidePopup = false); // Trauma added hidePopup
}
