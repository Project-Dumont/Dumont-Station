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
        [Dependency] private readonly SpriteSystem _sprite = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<SmallHeightComponent, ComponentStartup>(OnSmallHeightComponentStartup);
            SubscribeLocalEvent<BigHeightComponent, ComponentStartup>(OnBigHeightComponentStartup);
            SubscribeLocalEvent<SmallHeightComponent, ComponentShutdown>(OnSmallHeightComponentShutdown);
            SubscribeLocalEvent<BigHeightComponent, ComponentShutdown>(OnBigHeightComponentShutdown);
        }

        private void OnSmallHeightComponentStartup(Entity<SmallHeightComponent> ent, ref ComponentStartup args)
        {
            if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid) && CheckSpeciesEntity(humanoid))
                return;

            _sprite.SetScale(ent.Owner, new Vector2(0.85f, 0.85f));
        }

        private void OnBigHeightComponentStartup(Entity<BigHeightComponent> ent, ref ComponentStartup args)
        {
            if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid) && CheckSpeciesEntity(humanoid))
                return;

            _sprite.SetScale(ent.Owner, new Vector2(1.2f, 1.2f));
        }

        private void OnSmallHeightComponentShutdown(Entity<SmallHeightComponent> ent, ref ComponentShutdown args)
        {
            if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid) && CheckSpeciesEntity(humanoid))
                return;

            _sprite.SetScale(ent.Owner, new Vector2(1.0f, 1.0f));
        }

        private void OnBigHeightComponentShutdown(Entity<BigHeightComponent> ent, ref ComponentShutdown args)
        {
            if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid) && CheckSpeciesEntity(humanoid))
                return;

            _sprite.SetScale(ent.Owner, new Vector2(1.0f, 1.0f));
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
