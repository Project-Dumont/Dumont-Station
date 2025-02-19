using System.Linq;
using System.Numerics;
using Content.Shared.Height;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.Height
{
    public sealed class HeightSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SmallHeightComponent, ComponentStartup>(OnSmallHeightComponentStartup);
            SubscribeLocalEvent<BigHeightComponent, ComponentStartup>(OnBigHeightComponentStartup);
        }

        private void OnSmallHeightComponentStartup(Entity<SmallHeightComponent> ent, ref ComponentStartup args)
        {
            if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid) && CheckSpeciesEntity(humanoid))
                return;

            if (TryComp<SpriteComponent>(ent, out var sprite))
            {
                sprite.Scale = new Vector2(0.85f, 0.85f);
                Dirty(ent, sprite);
            }
        }

        private void OnBigHeightComponentStartup(Entity<BigHeightComponent> ent, ref ComponentStartup args)
        {
            if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid) && CheckSpeciesEntity(humanoid))
                return;

            if (TryComp<SpriteComponent>(ent, out var sprite))
            {
                sprite.Scale = new Vector2(1.2f, 1.2f);
                Dirty(ent, sprite);
            }
        }

        private bool CheckSpeciesEntity(HumanoidAppearanceComponent humanoid)
        {
            var allowedSpecies = new[]
            {
                new ProtoId<SpeciesPrototype>("Dwarf"),
                new ProtoId<SpeciesPrototype>("Felinid"),
                new ProtoId<SpeciesPrototype>("Resomi")
            };
            return allowedSpecies.Contains(humanoid.Species);
        }
    }
}
