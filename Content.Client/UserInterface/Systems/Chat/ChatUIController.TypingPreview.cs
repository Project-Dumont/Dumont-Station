// SPDX-FileCopyrightText: 2026 mirrorcult <19853115+mirrorcult@users.noreply.github.com>
// SPDX-License-Identifier: MIT

using Content.Client.Chat.UI;
using Content.Client.UserInterface.Systems.Chat.Widgets;
using Content.Shared.Chat;
using Content.Shared.CCVar;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Systems.Chat;

public sealed partial class ChatUIController
{
    private SpeechBubble? _typingPreview;
    private EntityUid? _typingPreviewEntity;
    private SpeechBubble.SpeechType _typingPreviewType;

    private void ClearTypingPreview()
    {
        _typingPreview?.Dispose();
        _typingPreview = null;
        _typingPreviewEntity = null;
    }

    public void UpdateTypingPreview(ChatBox box)
    {
        var (prefix, text, _, _) = SplitInputContents(box.ChatInput.Input.Text);
        var channel = prefix == ChatSelectChannel.None ? box.SelectedChannel : prefix;
        if (_player.LocalEntity is not { } entity || string.IsNullOrWhiteSpace(text)
            || channel is not (ChatSelectChannel.Local or ChatSelectChannel.Whisper or ChatSelectChannel.Emotes or ChatSelectChannel.Radio or ChatSelectChannel.LOOC))
        {
            ClearTypingPreview();
            return;
        }

        var type = channel switch
        {
            ChatSelectChannel.Whisper => SpeechBubble.SpeechType.Whisper,
            ChatSelectChannel.Emotes => SpeechBubble.SpeechType.Emote,
            ChatSelectChannel.LOOC => SpeechBubble.SpeechType.Looc,
            _ => SpeechBubble.SpeechType.Say,
        };
        var escaped = FormattedMessage.EscapeText(text);
        var message = new ChatMessage(ChatChannel.Local, escaped, channel is ChatSelectChannel.Emotes or ChatSelectChannel.LOOC ? escaped : $"[BubbleContent]{escaped}[/BubbleContent]", default, null);
        if (_typingPreview is { Disposed: false } && _typingPreviewEntity == entity && _typingPreviewType == type)
        {
            var style = _config.GetCVar(CCVars.ChatEnableRunechatBubbles)
                ? type switch
                {
                    SpeechBubble.SpeechType.Whisper => "runechatWhisper",
                    SpeechBubble.SpeechType.Emote => "runechatEmote",
                    SpeechBubble.SpeechType.Looc => "runechatLooc",
                    _ => "runechatSay",
                }
                : type switch
                {
                    SpeechBubble.SpeechType.Whisper => "whisperBox",
                    SpeechBubble.SpeechType.Emote or SpeechBubble.SpeechType.Looc => "emoteBox",
                    _ => "sayBox",
                };
            _typingPreview.RebuildContents(message, style);
            return;
        }
        ClearTypingPreview();
        _typingPreviewType = type;
        _typingPreview = SpeechBubble.CreateSpeechBubble(type, message, entity);
        _typingPreview.MakePermanent();
        _typingPreview.OnDied += (_, bubble) => bubble.Dispose();
        _typingPreviewEntity = entity;
        _speechBubbleRoot.AddChild(_typingPreview);
    }
}
