// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.Components;

[RegisterComponent]
public sealed partial class BloodCultLeaderFormComponent : Component
{
    [DataField]
    public EntProtoId SuitProto = "ClothingOuterBloodCultLeaderForm";

    [DataField]
    public EntProtoId HeadProto = "ClothingHeadBloodCultLeaderForm";

    [ViewVariables]
    public EntityUid? Suit;

    [ViewVariables]
    public EntityUid? Head;
}
