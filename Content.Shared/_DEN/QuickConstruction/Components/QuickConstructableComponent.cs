// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DEN.QuickConstruction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._DEN.QuickConstruction.Components;
/// <summary>
/// This component allows items to be interacted with to open a quick construction radial menu
/// containing a category of items to construct.
/// </summary>
[RegisterComponent]
public sealed partial class QuickConstructableComponent : Component
{
    [DataField]
    public ProtoId<QuickConstructionCategoryPrototype> Category;
}
