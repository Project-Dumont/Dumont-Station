// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._ES.Sabotage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HeavyGravityComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SpeedModifier = 0.7f;
}
