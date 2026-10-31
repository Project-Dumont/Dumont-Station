// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Antag.Components;

/// <summary>
/// Marks a spawn point on a rule grid as belonging to the listed antag roles only.
/// </summary>
[RegisterComponent]
public sealed partial class AntagGridSpawnPointComponent : Component
{
    /// <summary>
    /// The antag roles that are allowed to spawn here.
    /// </summary>
    [DataField]
    public List<ProtoId<AntagPrototype>> Whitelist = new();
}
