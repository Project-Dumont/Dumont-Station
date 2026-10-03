// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the subject to have an organ slot in a body part, filled or not.
/// </summary>
public sealed partial class HasOrganSlot : AbductorTaskCondition
{
    [DataField(required: true)]
    public string Slot = string.Empty;

    [DataField(required: true)]
    public BodyPartType PartType;

    [DataField]
    public BodyPartSymmetry? Symmetry;

    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        var body = entMan.System<SharedBodySystem>();
        foreach (var (_, part) in body.GetBodyChildrenOfType(target, PartType, symmetry: Symmetry))
        {
            if (part.Organs.ContainsKey(Slot))
                return true;
        }

        return false;
    }
}
