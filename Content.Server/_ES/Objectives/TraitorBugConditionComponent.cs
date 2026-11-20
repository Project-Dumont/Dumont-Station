// SPDX-FileCopyrightText: 2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Shared.Roles;
using Content.Shared.Access;
using Robust.Shared.Prototypes;

namespace Content.Server._ES.Objectives;

[RegisterComponent]
public sealed partial class TraitorBugConditionComponent : Component
{
    [DataField]
    public ProtoId<DepartmentPrototype>? Department;

    [DataField]
    public EntityUid? Station;

    [DataField(required: true)]
    public Dictionary<ProtoId<DepartmentPrototype>, ProtoId<AccessGroupPrototype>> DepartmentAccessGroups = new();

    [DataField]
    public List<ProtoId<AccessLevelPrototype>> Blacklist = new();
}
