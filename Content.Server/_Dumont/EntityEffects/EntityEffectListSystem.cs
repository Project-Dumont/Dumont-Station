using Content.Shared.EntityEffects;
using Robust.Shared.Random;

namespace Content.Server._Dumont.EntityEffects;

public sealed partial class EntityEffectListSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedEntityEffectSystem _effects = default!;

    public void ApplyEffects(EntityUid target, EntityEffect[] effects)
    {
        foreach (var effect in effects)
        {
            var args = new EntityEffectBaseArgs(target, EntityManager);
            if (!effect.ShouldApply(args, _random))
                continue;

            _effects.Effect(effect, args);
        }
    }
}
