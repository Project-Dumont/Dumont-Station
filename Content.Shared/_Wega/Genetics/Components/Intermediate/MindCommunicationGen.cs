// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Genetics;

[RegisterComponent]
public sealed partial class MindCommunicationGenComponent : Component
{
    [ValidatePrototypeId<EntityPrototype>]
    public readonly string Action = "ActionMindCommunicationGen";

    public EntityUid? ActionEntity { get; set; }
}
