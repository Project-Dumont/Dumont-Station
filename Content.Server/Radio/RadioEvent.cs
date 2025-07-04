// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 FoxxoTrystan <45297731+FoxxoTrystan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SkaldetSkaeg <impotekh@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Gabystation.Language;
using Content.Shared.Chat;
using Content.Shared.Radio;

namespace Content.Server.Radio;

/// <summary>
/// <param name="OriginalChatMsg">The message to display when the speaker can understand "language"</param>
/// <param name="LanguageObfuscatedChatMsg">The message to display when the speaker cannot understand "language"</param>
/// </summary>
[ByRefEvent]
public readonly record struct RadioReceiveEvent(
    // Gaby Station -> languages
    EntityUid MessageSource,
    RadioChannelPrototype Channel,
    EntityUid RadioSource,
    ChatMessage OriginalChatMsg, // Gaby Station -> Languages
    ChatMessage LanguageObfuscatedChatMsg, // Gaby Station -> Languages
    LanguagePrototype Language
);

/// <summary>
/// Use this event to cancel sending message per receiver
/// </summary>
[ByRefEvent]
public record struct RadioReceiveAttemptEvent(RadioChannelPrototype Channel, EntityUid RadioSource, EntityUid RadioReceiver)
{
    public readonly RadioChannelPrototype Channel = Channel;
    public readonly EntityUid RadioSource = RadioSource;
    public readonly EntityUid RadioReceiver = RadioReceiver;
    public bool Cancelled = false;
}

/// <summary>
/// Use this event to cancel sending message to every receiver
/// </summary>
[ByRefEvent]
public record struct RadioSendAttemptEvent(RadioChannelPrototype Channel, EntityUid RadioSource)
{
    public readonly RadioChannelPrototype Channel = Channel;
    public readonly EntityUid RadioSource = RadioSource;
    public bool Cancelled = false;
}