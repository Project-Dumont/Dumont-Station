// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Hailer;

[Prototype]
public sealed partial class HailerEntryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// O áudio que será tocado ao falar essa entrada.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound = default!;

    /// <summary>
    /// A mensagem que será mostrada ao falar essa entrada.
    /// </summary>
    [DataField]
    public LocId Message = default!;
}