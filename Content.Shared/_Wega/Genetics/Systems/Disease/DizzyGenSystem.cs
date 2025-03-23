using Content.Shared.Drunk;
using Content.Shared.StatusEffect;
using Robust.Shared.Timing;

namespace Content.Shared.Genetics.Systems;

public sealed class DizzySystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    [ValidatePrototypeId<StatusEffectPrototype>]
    public const string DizzyKey = "Dizzy";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DizzyGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DizzyGenComponent, ComponentShutdown>(OnShutdown);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DizzyEffectComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_statusEffectsSystem.TryGetTime(uid, DizzyKey, out var time))
            {
                if (time.Value.Item2 - _gameTiming.CurTime < TimeSpan.FromMinutes(1))
                    _statusEffectsSystem.TryAddTime(uid, DizzyKey, TimeSpan.FromMinutes(10));
            }
        }
    }

    private void OnInit(Entity<DizzyGenComponent> ent, ref ComponentInit args)
    {
        if (!_statusEffectsSystem.HasStatusEffect(ent, DizzyKey))
        {
            EnsureComp<DizzyEffectComponent>(ent, out var dizzyEffect);
            _statusEffectsSystem.TryAddStatusEffect<DrunkComponent>(ent, DizzyKey, TimeSpan.FromMinutes(10), true);

            dizzyEffect.Intensity = ent.Comp.InitialIntensity;
        }
    }

    private void OnShutdown(Entity<DizzyGenComponent> ent, ref ComponentShutdown args)
    {
        RemComp<DizzyEffectComponent>(ent);
        _statusEffectsSystem.TryRemoveStatusEffect(ent, DizzyKey);
    }
}

