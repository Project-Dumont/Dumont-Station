// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid;

[Virtual]
public partial class MarkingColoring : LayerColoringType
{
    [DataField(required: true)]
    public int ColorIndex;

    [DataField(required: true)]
    public ProtoId<MarkingPrototype> MarkingId;

    [DataField(required: true)]
    public MarkingCategories MarkingCategory;

    public override Color? GetCleanColor(Color? skin, Color? eyes, MarkingSet markingSet)
    {
        return markingSet.Markings.GetValueOrDefault(MarkingCategory)
            ?.FirstOrDefault(x => x.MarkingId == MarkingId)
            ?.MarkingColors.ElementAtOrDefault(ColorIndex);
    }
}
