// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.ADT.Droppods.Components;

/// <summary>
/// When a <c>TimedDespawnComponent"</c> despawns, another one will be spawned in its place.
/// </summary>
[RegisterComponent] //ADT Tweak no access
public sealed partial class DroppodComponent : Component
{
    /// <summary>
    /// protos to spawn
    /// </summary>
    [DataField]
    public List<EntProtoId> Prototypes { get; set; } = new();
}