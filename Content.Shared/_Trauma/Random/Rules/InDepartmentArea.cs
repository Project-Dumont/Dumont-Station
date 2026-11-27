// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Random.Rules;
using Content.Shared.Roles;
using Content.Trauma.Shared.Areas;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Random.Rules;

/// <summary>
/// Returns true if the attached entity is inside an area's department
/// </summary>
public sealed partial class InDepartmentAreaRule : RulesRule
{
    [DataField(required: true)]
    public ProtoId<DepartmentPrototype> Department;

    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        var areaSys = entManager.System<AreaSystem>();

        return (areaSys.GetArea(uid) is { } area
               && areaSys.GetAreaDepartment(area) is { } areaDepartment
               && areaDepartment == Department)
               != Inverted;
    }
}
