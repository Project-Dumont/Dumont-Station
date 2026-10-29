using System.Linq;
using Content.Goobstation.Client.Clothing.Components;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Clothing.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Goobstation.Heretic;

[TestFixture]
public sealed class VaticanEquipmentTests
{
    [Test]
    public async Task InquisitoryClothingReferencesExistingSprites()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        await pair.Client.WaitAssertion(() =>
        {
            var em = pair.Client.EntMan;
            var factory = pair.Client.ResolveDependency<IComponentFactory>();
            foreach (var prototype in new[] { "ClothingModsuitInquisitory", "ClothingModsuitHelmetInquisitory",
                         "ClothingModsuitChestplateInquisitory", "ClothingModsuitGauntletsInquisitory", "ClothingModsuitBootsInquisitory" })
            {
                var proto = pair.Client.ProtoMan.Index<EntityPrototype>(prototype);
                Assert.That(proto.TryGetComponent<SpriteComponent>(out var sprite, factory));
                Assert.That(proto.TryGetComponent<ClothingComponent>(out var clothing, factory));
                Assert.That(proto.TryGetComponent<SealableClothingVisualsComponent>(out var sealedVisuals, factory));
                foreach (var layer in clothing.ClothingVisuals.Values.Concat(sealedVisuals.ClothingVisuals.Values).SelectMany(l => l))
                {
                    if (layer.RsiPath != null || layer.State == null)
                        continue;
                    Assert.That(sprite.BaseRSI!.TryGetState(layer.State, out _), Is.True,
                        $"{prototype}: missing equipped state {layer.State}");
                }
            }
        });
        await pair.CleanReturnAsync();
    }

    [TestCase("VaticanDueProcess")]
    [TestCase("VaticanThemis")]
    public async Task PrayerRestoresFiniteAmmo(string prototype)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
        await pair.Server.WaitAssertion(() =>
        {
            var em = pair.Server.EntMan;
            var uid = em.SpawnEntity(prototype, MapCoordinates.Nullspace);
            var ammo = em.GetComponent<BasicEntityAmmoProviderComponent>(uid);
            var take = new TakeAmmoEvent(ammo.Capacity!.Value + 1, new(), new EntityCoordinates(uid, default), null);
            em.EventBus.RaiseLocalEvent(uid, take);
            Assert.That(take.Ammo.Count, Is.EqualTo(ammo.Capacity));
            Assert.That(ammo.Count, Is.Zero);
            foreach (var (projectile, _) in take.Ammo)
                em.DeleteEntity(projectile!.Value);
            var prayer = new AlternatePrayEvent(uid);
            em.EventBus.RaiseLocalEvent(uid, ref prayer);
            Assert.That(ammo.Count, Is.EqualTo(ammo.Capacity));
            em.DeleteEntity(uid);
        });
        await pair.CleanReturnAsync();
    }
}
