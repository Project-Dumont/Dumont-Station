// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Content.Shared.Access;

namespace Content.Shared.PDA;

/// <summary>
/// Defines a group of pdas to notify based on the access in its inserted ID, an entirely empty group meaning that it encompasses all PDA.
/// A given pda will be notified if its id's access are contained in the notification group's accesses
/// </summary>

[DataDefinition]
[Prototype]
public sealed partial class NotificationGroupPrototype : IPrototype {
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public HashSet<ProtoId<AccessGroupPrototype>>? AccessGroups = null;

    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>>? Access = null;

    /// <summary>
    /// Excludes PDAs. A given notification group will represent a
    /// range of PDAs which both have access as defined by AccessGroups
    /// and Access and do not have accesses in the Exclude field.
    /// If both AcessGroups and Access are null (i.e the group
    /// emcompases all PDAs) then Exclude is not applicabe
    /// </summary>
    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>>? Exclude = null;

}
