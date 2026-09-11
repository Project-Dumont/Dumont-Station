// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;

[RegisterComponent, NetworkedComponent]
public sealed partial class BladeArenaComponent : Component
{
    [DataField]
    public ProtoId<TagPrototype> WindowTag = "Window";

    [DataField]
    public List<ProtoId<TagPrototype>> NoReplaceTags = new()
    {
        "Diagonal",
        "Directional",
    };

    [DataField]
    public ProtoId<ContentTileDefinition> Tile = "PlatingRoseStone";

    [DataField]
    public EntProtoId WallReplacement = "WallHereticArena";

    [DataField]
    public EntProtoId<BladeArenaOuterWallComponent> OuterWall = "WallHereticArenaOuter";

    [DataField]
    public EntProtoId WindowReplacement = "WindowHereticArena";

    [DataField]
    public int Radius;

    [DataField]
    public EntityUid? Grid;

    [DataField]
    public List<EntityUid> DetachedEntities = new();

    [DataField]
    public List<EntityUid> SpawnedEntities = new();

    [DataField]
    public List<Vector2i> TilesToRestore = new();

    [DataField]
    public HashSet<EntityUid> Participants = new();

    [DataField]
    public int Layer = (int) (CollisionGroup.Impassable | CollisionGroup.HighImpassable |
                                       CollisionGroup.MidImpassable | CollisionGroup.LowImpassable);

    [DataField]
    public ComponentRegistry ComponentsToAdd = new();

    [DataField]
    public EntityWhitelist? ParticipantWhitelist;

    [DataField]
    public EntityWhitelist? ParticipantBlacklist;
}
