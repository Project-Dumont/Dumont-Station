// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Dumont.Roles;

/// <summary>
/// Dados dos créditos de fim de rodada de um antag. O ID tem que ser o mesmo do antag.
/// </summary>
[Prototype]
public sealed partial class AntagCreditsPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The image to be displayed in the end credits, if empty it will be text instead
    /// </summary>
    [DataField]
    public ResPath? CreditImage { get; private set; }

    /// <summary>
    /// The color associated with this antag
    /// </summary>
    [DataField]
    public Color Color { get; private set; } = Color.Red;

    /// <summary>
    /// Use this to hide the slop stuff like nuke ops command, med, ect
    /// </summary>
    [DataField]
    public bool DontShowInCredits { get; private set; }
}
