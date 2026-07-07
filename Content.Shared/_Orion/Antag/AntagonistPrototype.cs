// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Orion.Antag;

[Prototype("antagonist")]
public sealed partial class AntagonistPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    ///     Name string to display in ghost teleport menu .
    /// </summary>
    [DataField(required: true)]
    public string Name = default!;

    /// <summary>
    ///     Description string to display in the character menu as an explanation of the department's function.
    /// </summary>
    [DataField(required: true)]
    public string Description = default!;

    /// <summary>
    ///     Description string to display in the character menu as an explanation of the department's function.
    /// </summary>
    [DataField(required: true)]
    public int Weight;
}
