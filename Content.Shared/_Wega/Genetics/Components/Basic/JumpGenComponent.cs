// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class JumpGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public readonly string ActionId = "ActionJumpGen";

    [DataField]
    public EntityUid? ActionEntity;
}
