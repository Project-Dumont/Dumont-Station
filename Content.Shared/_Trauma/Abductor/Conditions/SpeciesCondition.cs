// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the subject to be a humanoid of a given species.
/// </summary>
public sealed partial class SpeciesCondition : AbductorTaskCondition
{
    [DataField(required: true)]
    public ProtoId<SpeciesPrototype> Species;

    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        return entMan.TryGetComponent<HumanoidAppearanceComponent>(target, out var humanoid) &&
            humanoid.Species == Species;
    }
}
