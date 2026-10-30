using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using Content.Client._Lavaland.DeployableGrid;
using Content.Shared._Lavaland.DeployableGrid;
using Content.Shared.GridPreloader.Prototypes;
using Robust.Client.GameObjects;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;
using Robust.Shared.Maths;
using YamlDotNet.RepresentationModel;

namespace Content.IntegrationTests.Tests._Lavaland;

[TestFixture]
public sealed class ShelterGhostTest
{
    /// <summary>
    /// The cached preview must retain the actual map's floors, furniture positions and directions.
    /// </summary>
    [TestCase("ShelterCapsule", 5)]
    [TestCase("ShelterCapsuleLuxury", 7)]
    public async Task PreviewMatchesMapAndReusesScene(string capsule, int size)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        await pair.Server.WaitAssertion(() =>
        {
            var prototypes = pair.Server.ResolveDependency<IPrototypeManager>();
            var component = prototypes.Index<EntityPrototype>(capsule).Components["DeployableGrid"].Component as DeployableGridComponent;
            Assert.That(component, Is.Not.Null);
            Assert.That(component!.Ghost, Is.Not.Null);
            var layout = prototypes.Index(component.Ghost!.Value);
            Assert.That(layout.Size, Is.EqualTo(new Vector2i(size, size)));
            Assert.That((Vector2) layout.Size, Is.EqualTo(component.BoxSize));
            Assert.That(layout.SourceMap, Is.EqualTo(prototypes.Index(component.PreloadedGrid).Path));

            using var reader = pair.Server.ResolveDependency<IResourceManager>().ContentFileReadText(layout.SourceMap);
            var yaml = new YamlStream();
            yaml.Load(reader);
            var root = (YamlMappingNode) yaml.Documents[0].RootNode;
            var gridId = ((YamlSequenceNode) root["grids"]).Children[0].ToString();
            var tiles = (YamlMappingNode) root["tilemap"];
            var expectedParts = layout.Entities.Select(part => (part.Prototype.Id, part.Offset, part.Rotation.Theta)).ToList();
            var floorCount = 0;
            foreach (YamlMappingNode group in (YamlSequenceNode) root["entities"])
            foreach (YamlMappingNode entity in (YamlSequenceNode) group["entities"])
            {
                var components = ((YamlSequenceNode) entity["components"]).Children.Cast<YamlMappingNode>().ToArray();
                if (entity["uid"].ToString() == gridId)
                {
                    var grid = components.Single(comp => comp["type"].ToString() == "MapGrid");
                    foreach (YamlMappingNode chunk in ((YamlMappingNode) grid["chunks"]).Children.Values)
                    {
                        Assert.That(chunk["version"].ToString(), Is.EqualTo("7"), "Update the preview check if the map format changes.");
                        var indices = chunk["ind"].ToString().Split(',').Select(int.Parse).ToArray();
                        using var binary = new BinaryReader(new MemoryStream(Convert.FromBase64String(chunk["tiles"].ToString())));
                        for (var y = 0; y < 16; y++)
                        for (var x = 0; x < 16; x++)
                        {
                            var tile = binary.ReadInt32();
                            binary.ReadByte(); // Flags do not affect the preview.
                            var variant = binary.ReadByte();
                            var rotation = binary.ReadByte();
                            if (tile == 0)
                                continue;
                            Assert.That(variant, Is.Zero, "Preview floors currently use the default variant.");
                            Assert.That(rotation, Is.Zero, "Preview floors currently use the default rotation.");
                            var tx = indices[0] * 16 + x;
                            var ty = indices[1] * 16 + y;
                            Assert.That(layout.Tiles[ty][tx], Is.EqualTo(tiles[tile.ToString(CultureInfo.InvariantCulture)].ToString()));
                            floorCount++;
                        }
                    }
                    continue;
                }

                var prototype = group["proto"].ToString();
                var transform = components.SingleOrDefault(comp => comp["type"].ToString() == "Transform");
                if (prototype.Length == 0 || transform == null ||
                    !transform.Children.TryGetValue("parent", out var parent) || parent.ToString() != gridId)
                    continue;
                var position = transform.Children.TryGetValue("pos", out var pos) ? pos.ToString() : "0,0";
                var xy = position.Split(',').Select(value => float.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                var angle = transform.Children.TryGetValue("rot", out var rot)
                    ? double.Parse(rot.ToString().Replace(" rad", ""), CultureInfo.InvariantCulture)
                    : 0;
                var index = expectedParts.FindIndex(part => part.Id == prototype &&
                    Vector2.Distance(part.Offset, new Vector2(xy[0], xy[1])) < 0.001f &&
                    Math.Abs(part.Theta - angle) < 0.001);
                Assert.That(index, Is.GreaterThanOrEqualTo(0), $"Missing preview entity: {prototype} at {position}.");
                expectedParts.RemoveAt(index);
            }

            Assert.That(floorCount, Is.EqualTo(size * size));
            Assert.That(expectedParts, Is.Empty, "Preview contains entities absent from the actual map.");
        });

        await pair.Client.WaitAssertion(() =>
        {
            using var renderer = new ShelterGhostRenderer(pair.Client.EntMan);
            var id = new ProtoId<ShelterGhostPrototype>($"ShelterCapsule{size}x{size}");
            var scene = renderer.Prepare(id);
            Assert.That(scene.Size, Is.EqualTo(new Vector2i(size, size)));
            Assert.That(scene.Pieces.Count, Is.GreaterThan(size * size), "Furniture sprites must be included.");
            Assert.That(renderer.Prepare(id), Is.SameAs(scene), "Repeated inspections must reuse the prepared layout.");
            Assert.That(scene.Pieces.All(piece => piece.Size.X > 0 && piece.Size.Y > 0), Is.True);

            var prototypes = pair.Client.ResolveDependency<IPrototypeManager>();
            var markers = prototypes.Index(id).Entities
                .Where(part => prototypes.Index(part.Prototype).Components.ContainsKey("Marker")).ToArray();
            Assert.That(markers.Length, Is.EqualTo(size == 7 ? 3 : 0),
                "The luxury shelter's real layout must retain its three random spawners.");
            var sprites = pair.Client.System<SpriteSystem>();
            foreach (var marker in markers)
            {
                var icon = sprites.GetPrototypeIcon(marker.Prototype.Id).Default;
                Assert.That(scene.Pieces.Any(piece => ReferenceEquals(piece.Texture, icon)), Is.False,
                    $"Editor marker {marker.Prototype} must not be drawn in the preview.");
            }
        });
        await pair.RunTicksSync(2);
        await pair.CleanReturnAsync();
    }
}
