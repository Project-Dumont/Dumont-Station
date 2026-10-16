// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class MorphismGenComponent : Component
{
    [DataField, ValidatePrototypeId<EntityPrototype>]
    public string ActionId = "ActionMorphismGen";

    [DataField]
    public EntityUid? ActionEntity;
}
