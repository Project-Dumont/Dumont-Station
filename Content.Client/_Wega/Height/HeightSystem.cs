using System.Numerics;
using Content.Shared.Height;
using Robust.Client.GameObjects;

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

        private void OnSmallHeightComponentStartup(EntityUid uid, SmallHeightComponent comp, ComponentStartup args)
        {
            if (TryComp<SpriteComponent>(uid, out var sprite))
            {
                sprite.Scale = new Vector2(0.8f, 0.8f);
                Dirty(uid, sprite);
            }
        }

        private void OnBigHeightComponentStartup(EntityUid uid, BigHeightComponent comp, ComponentStartup args)
        {
            if (TryComp<SpriteComponent>(uid, out var sprite))
            {
                sprite.Scale = new Vector2(1.2f, 1.2f);
                Dirty(uid, sprite);
            }
        }
    }
}
