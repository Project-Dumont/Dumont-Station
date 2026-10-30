using System.Collections.Generic;
using System.Numerics;
using Content.Shared._Lavaland.DeployableGrid;
using Content.Shared.Maps;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.Utility;
using Robust.Shared.Map;
using Robust.Shared.Graphics;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Prototypes;
using ContentDrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._Lavaland.DeployableGrid;

/// <summary>
/// Composes each shelter once, then reuses its texture while examining capsules.
/// </summary>
public sealed class ShelterGhostRenderer(IEntityManager entities) : IDisposable
{
    private readonly Dictionary<string, Scene> _scenes = new();
    private readonly Dictionary<string, IRenderTexture> _textures = new();

    public sealed record Piece(Texture Texture, Vector2 Position, Vector2 Size, float Rotation, int Depth);
    public sealed record Scene(Vector2i Size, List<Piece> Pieces);

    public Scene Prepare(ProtoId<ShelterGhostPrototype> id)
    {
        if (_scenes.TryGetValue(id.Id, out var cached))
            return cached;

        var prototypes = IoCManager.Resolve<IPrototypeManager>();
        var resources = IoCManager.Resolve<IResourceCache>();
        var tiles = IoCManager.Resolve<ITileDefinitionManager>();
        var components = IoCManager.Resolve<IComponentFactory>();
        var sprites = entities.System<SpriteSystem>();
        var layout = prototypes.Index(id);
        var pieces = new List<Piece>();
        for (var y = 0; y < layout.Size.Y; y++)
        for (var x = 0; x < layout.Size.X; x++)
        {
            var tile = (ContentTileDefinition) tiles[layout.Tiles[y][x]];
            if (tile.Sprite is not {} path)
                continue;
            var texture = resources.GetResource<TextureResource>(path).Texture;
            pieces.Add(new Piece(new AtlasTexture(texture, new UIBox2(0, 0, 32, 32)),
                new Vector2(x + 0.5f, y + 0.5f), Vector2.One, 0, (int) ContentDrawDepth.FloorTiles - 1));
        }

        foreach (var part in layout.Entities)
        {
            var prototype = prototypes.Index(part.Prototype);
            // Editor markers describe spawns, not visible objects in the deployed shelter.
            if (prototype.Components.ContainsKey("Marker"))
                continue;

            if (!prototype.TryGetComponent<SpriteComponent>(out var sprite, components) ||
                !sprite.Visible || sprite.DrawDepth < (int) ContentDrawDepth.FloorTiles)
                continue;

            var icon = sprites.GetPrototypeIcon(part.Prototype.Id);
            var texture = icon.GetFrame(part.Rotation.ToRsiDirection(icon.RsiDirections), 0);
            var rotation = icon.RsiDirections == RsiDirectionType.Dir1 && !sprite.NoRotation ? -(float) part.Rotation.Theta : 0;
            pieces.Add(new Piece(texture, part.Offset + part.Rotation.RotateVec(sprite.Offset),
                (Vector2) texture.Size / 32f * sprite.Scale, rotation, sprite.DrawDepth));
        }

        pieces.Sort((a, b) => a.Depth.CompareTo(b.Depth));
        var scene = new Scene(layout.Size, pieces);
        _scenes.Add(id.Id, scene);
        return scene;
    }

    public Texture GetTexture(ProtoId<ShelterGhostPrototype> id, IRenderHandle render)
    {
        if (_textures.TryGetValue(id.Id, out var cached))
            return cached.Texture;

        var scene = Prepare(id);
        // A tile of padding preserves sprites extending beyond the footprint.
        var target = IoCManager.Resolve<IClyde>().CreateRenderTarget(
            (scene.Size + new Vector2i(2, 2)) * 32,
            new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
            new TextureSampleParameters { Filter = false });
        var screen = render.DrawingHandleScreen;
        try
        {
            screen.RenderInRenderTarget(target, () =>
            {
                screen.UseShader(null);
                foreach (var piece in scene.Pieces)
                {
                    var center = new Vector2((piece.Position.X + 1) * 32, (scene.Size.Y + 1 - piece.Position.Y) * 32);
                    screen.SetTransform(Matrix3x2.CreateRotation(piece.Rotation, center));
                    var halfSize = piece.Size * 16;
                    screen.DrawTextureRect(piece.Texture, new UIBox2(center - halfSize, center + halfSize));
                }
                screen.SetTransform(Matrix3x2.Identity);
            }, Color.Transparent);
        }
        catch
        {
            target.Dispose();
            throw;
        }
        _textures.Add(id.Id, target);
        return target.Texture;
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
            texture.Dispose();
        _textures.Clear();
        _scenes.Clear();
    }
}
