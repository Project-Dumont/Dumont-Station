// SPDX-FileCopyrightText: 2025 cosmosgc <cosmoskitsune@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Language;

namespace Content.Server.Andromeda.TTS;

public sealed class RadioSpokeEvent : EntityEventArgs
{
    public EntityUid Source { get; set; }
    public string Message { get; set; } = null!;
    public EntityUid[] Receivers { get; set; } = null!;
    public LanguagePrototype Language { get; set; } = new LanguagePrototype();
}
