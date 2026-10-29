// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Map;
// Dumont end

namespace Content.Trauma.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class RustSpreaderComponent : Component
{
    [NonSerialized]
    public Queue<TileRef> TilesToRust = new();

    [NonSerialized]
    public HashSet<TileRef> ProcessedTiles = new();

    [NonSerialized]
    public HashSet<EntityUid> AffectedDocks = new();

    [DataField]
    public int AmountToRust = 1;

    [DataField]
    public bool Paused;

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}
