using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Body.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared.Body.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class BloodTypeComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<BloodTypePrototype>? Type;

    [DataField, AutoNetworkedField]
    public FixedPoint2 ForeignBloodDeducted = 1.0f;
}

