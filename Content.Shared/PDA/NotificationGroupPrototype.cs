// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Content.Shared.Access;

namespace Content.Shared.PDA;

[Prototype]
public sealed class NotificationGroupPrototype : IPrototype {
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public HashSet<ProtoId<AccessGroupPrototype>>? AccessGroups = null;

    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>>? Access = null;

}
