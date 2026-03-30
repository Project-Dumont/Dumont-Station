using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Gabystation.Interaction.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RecallBoundItemComponent : Component
{
    /// <summary>
    /// The item bound to this user.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? BoundItem;

    /// <summary>
    /// Prototype of the recall action.
    /// </summary>
    [DataField]
    public EntProtoId RecallAction = "ActionRecallBoundItem";

    /// <summary>
    /// The spawned action entity.
    /// </summary>
    public EntityUid? RecallActionEntity;
}
