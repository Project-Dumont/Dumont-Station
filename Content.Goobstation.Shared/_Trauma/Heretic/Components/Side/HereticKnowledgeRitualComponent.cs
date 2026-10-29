// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Prototypes;
using Content.Trauma.Shared.Heretic.Rituals;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Side;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HereticKnowledgeRitualComponent : Component
{
    [DataField(required: true)]
    public Dictionary<ProtoId<RitualIngredientDatasetPrototype>, int> Datasets;

    [DataField, AutoNetworkedField]
    public List<RitualIngredient> Ingredients = new();
}
