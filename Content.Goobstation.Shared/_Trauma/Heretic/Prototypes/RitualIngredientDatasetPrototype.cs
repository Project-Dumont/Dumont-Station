// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Rituals;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Prototypes;

[Prototype]
public sealed partial class RitualIngredientDatasetPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public RitualIngredient[] Ingredients = default!;
}
