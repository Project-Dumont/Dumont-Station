// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation.Speech.Prototypes;

[Prototype("profanityWords")]
public sealed partial class ProfanityFilterAccentPrototype : IPrototype
{
    /// <inheritdoc/>
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// A list of bad words to be censored
    /// </summary>
    [DataField]
    public List<string>? Words;
}
