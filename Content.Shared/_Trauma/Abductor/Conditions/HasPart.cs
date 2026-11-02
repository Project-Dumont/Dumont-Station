// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the subject to have a body part of some type.
/// Optionally checks nested conditions against the part itself.
/// </summary>
public sealed partial class HasPart : AbductorTaskCondition
{
    [DataField(required: true)]
    public BodyPartType PartType;

    [DataField]
    public BodyPartSymmetry? Symmetry;

    /// <summary>
    /// Conditions the part must meet.
    /// </summary>
    [DataField]
    public AbductorTaskCondition[]? Conditions;

    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        var body = entMan.System<SharedBodySystem>();
        foreach (var (part, _) in body.GetBodyChildrenOfType(target, PartType, symmetry: Symmetry))
        {
            if (All(Conditions, part, entMan))
                return true;
        }

        return false;
    }
}
