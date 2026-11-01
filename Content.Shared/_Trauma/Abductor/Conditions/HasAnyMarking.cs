// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Requires the subject to have a given part and any marking on a given layer.
/// </summary>
public sealed partial class HasAnyMarking : AbductorTaskCondition
{
    /// <summary>
    /// The part the marking sits on.
    /// </summary>
    [DataField(required: true)]
    public BodyPartType PartType;

    /// <summary>
    /// The layer a marking must match.
    /// </summary>
    [DataField(required: true)]
    public HumanoidVisualLayers Layer;

    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        if (!entMan.TryGetComponent<HumanoidAppearanceComponent>(target, out var humanoid))
            return false;

        var body = entMan.System<SharedBodySystem>();
        if (body.GetBodyPartCount(target, PartType) == 0)
            return false; // no part lol

        var category = MarkingCategoriesConversion.FromHumanoidVisualLayers(Layer);
        return humanoid.MarkingSet.Markings.TryGetValue(category, out var list) && list.Count > 0;
    }
}
