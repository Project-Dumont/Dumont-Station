using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.DeployableGrid;

/// <summary>
/// Visual-only layout of a shelter; it never creates a grid or gameplay entities.
/// </summary>
[Prototype("shelterGhost")]
public sealed partial class ShelterGhostPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = string.Empty;
    [DataField(required: true)] public ResPath SourceMap;
    [DataField(required: true)] public Vector2i Size;
    [DataField(required: true)] public List<List<string>> Tiles = new();
    [DataField] public List<ShelterGhostEntity> Entities = new();
}

[DataDefinition]
public sealed partial class ShelterGhostEntity
{
    [DataField(required: true)] public EntProtoId Prototype;
    [DataField(required: true)] public Vector2 Offset;
    [DataField] public Angle Rotation;
}
