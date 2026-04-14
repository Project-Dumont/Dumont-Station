using Content.Shared.Drunk;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.Genetics.Systems;

// TODO: ref this
public sealed class DizzySystem : EntitySystem
{
    [Dependency] private readonly Content.Shared.StatusEffectNew.StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public static readonly ProtoId<StatusEffectPrototype> DizzyKey = "Dizzy";

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
            if (_statusEffectsSystem.TryGetTime(uid, DizzyKey.Id, out var time))
            {
                if (time.EndEffectTime - _gameTiming.CurTime < TimeSpan.FromMinutes(1))
                    _statusEffectsSystem.TryAddTime(uid, DizzyKey.Id, TimeSpan.FromMinutes(10));
            }
        }
    }

    private void OnInit(Entity<DizzyGenComponent> ent, ref ComponentInit args)
    {
        if (!_statusEffectsSystem.HasStatusEffect(ent, DizzyKey.Id))
        {
            EnsureComp<DizzyEffectComponent>(ent, out var dizzyEffect);
            _statusEffectsSystem.TryAddStatusEffect(ent, DizzyKey.Id, out _, TimeSpan.FromMinutes(10));

            dizzyEffect.Intensity = ent.Comp.InitialIntensity;
        }
    }

    private void OnShutdown(Entity<DizzyGenComponent> ent, ref ComponentShutdown args)
    {
        RemComp<DizzyEffectComponent>(ent);
        _statusEffectsSystem.TryRemoveStatusEffect(ent, DizzyKey.Id);
    }
}
