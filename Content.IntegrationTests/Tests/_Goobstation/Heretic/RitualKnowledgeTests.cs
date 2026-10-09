// SPDX-FileCopyrightText: 2024 TGRCDev <tgrc@tgrc.dev>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Content.Trauma.Shared.Heretic.Components.Side;
using Content.Trauma.Shared.Heretic.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture, TestOf(typeof(HereticKnowledgeRitualComponent))]
public sealed class RitualKnowledgeTests
{
    [Test]
    public async Task RitualsInitialize()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        var serialization = server.ResolveDependency<ISerializationManager>();
        var context = new PrototypeSaveTest.TestEntityUidContext(serialization);
        await server.WaitAssertion(() =>
        {
            var prototypes = server.ProtoMan.EnumeratePrototypes<EntityPrototype>()
                .Where(value => !value.Abstract && value.Components.ContainsKey("HereticRitual"));
            Assert.Multiple(() =>
            {
                foreach (var knowledge in server.ProtoMan.EnumeratePrototypes<HereticKnowledgePrototype>())
                {
                    var node = serialization.WriteValue(knowledge, alwaysWrite: true, context: context);
                    var errors = serialization.ValidateNode<HereticKnowledgePrototype>(node, context: context).GetErrors();
                    Assert.That(errors.Select(error => error.ErrorReason), Is.Empty, knowledge.ID);
                }
                foreach (var prototype in prototypes)
                {
                    var node = serialization.WriteValue(prototype, alwaysWrite: true, context: context);
                    var errors = serialization.ValidateNode<EntityPrototype>(node, context: context).GetErrors();
                    Assert.That(errors.Select(error => error.ErrorReason), Is.Empty, prototype.ID);
                    var ritual = server.EntMan.SpawnEntity(prototype.ID, map.MapCoords);
                    server.EntMan.DeleteEntity(ritual);
                }
            });
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("KnowledgeRitualOrgans", "arm", "LeftArmHuman")]
    [TestCase("KnowledgeRitualOrgans", "leg", "RightLegHuman")]
    [TestCase("KnowledgeRitualEasy", "wood-plank", "MaterialWoodPlank")]
    [TestCase("KnowledgeRitualEasy", "gold", "IngotGold")]
    [TestCase("KnowledgeRitualEasy", "silver", "IngotSilver")]
    [TestCase("KnowledgeRitualEasy", "glass-shard", "ShardGlass")]
    [TestCase("KnowledgeRitualHard", "gloves-medical", "ClothingHandsGlovesLatex")]
    [TestCase("KnowledgeRitualHard", "gloves-medical", "ClothingHandsGlovesNitrile")]
    [TestCase("KnowledgeRitualHard", "circular-saw", "SawElectric")]
    [TestCase("KnowledgeRitualHard", "combat-boots", "ClothingShoesBootsCombat")]
    public async Task StationItemMatchesIngredient(string datasetId, string ingredientName, string prototype)
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        await server.WaitAssertion(() =>
        {
            var ingredient = server.ProtoMan.Index<RitualIngredientDatasetPrototype>(datasetId).Ingredients
                .Single(value => value.Name.ToString() == $"heretic-ritual-ingredient-{ingredientName}");
            var item = server.EntMan.SpawnEntity(prototype, map.GridCoords);
            var whitelist = server.EntMan.System<EntityWhitelistSystem>();
            Assert.That(whitelist.IsValid(ingredient.Whitelist, item), Is.True, prototype);
            Assert.That(whitelist.IsBlacklistPass(ingredient.Blacklist, item), Is.False, prototype);
            server.EntMan.DeleteEntity(item);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task ValidateIngredientDatasets()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var factory = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            foreach (var dataset in server.ProtoMan.EnumeratePrototypes<RitualIngredientDatasetPrototype>())
            {
                Assert.That(dataset.Ingredients, Is.Not.Empty, dataset.ID);
                foreach (var ingredient in dataset.Ingredients)
                {
                    Assert.That(ingredient.Amount, Is.GreaterThan(0), dataset.ID);
                    foreach (var whitelist in new[] { ingredient.Whitelist, ingredient.Blacklist })
                    {
                        if (whitelist == null)
                            continue;
                        foreach (var component in whitelist.Components ?? [])
                            Assert.That(factory.GetRegistration(component), Is.Not.Null, dataset.ID);
                        foreach (var tag in whitelist.Tags ?? [])
                            Assert.That(server.ProtoMan.HasIndex<TagPrototype>(tag), Is.True, dataset.ID);
                    }
                }
            }
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task KnowledgeRitualSelectsIngredients()
    {
        await using var pair = await PoolManager.GetServerClient();
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        await server.WaitAssertion(() =>
        {
            var entity = server.EntMan.SpawnEntity("RitualKnowledge", map.MapCoords);
            var ritual = server.EntMan.GetComponent<HereticKnowledgeRitualComponent>(entity);
            Assert.That(ritual.Ingredients.Count, Is.EqualTo(ritual.Datasets.Values.Sum()));
            foreach (var ingredient in ritual.Ingredients)
            {
                Assert.That(ritual.Datasets.Keys.Any(id =>
                    server.ProtoMan.Index(id).Ingredients.Contains(ingredient)), Is.True);
            }
            server.EntMan.DeleteEntity(entity);
        });
        await pair.CleanReturnAsync();
    }
}
