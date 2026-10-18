using System.Linq;
using System.Numerics;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Spider;
using Content.Shared.Sprite;
using Content.Shared.Weapons.Melee;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Systems;

namespace Content.Server._Dumont.StationTraits;

public sealed partial class GiantSpidersRuleSystem : GameRuleSystem<GiantSpidersRuleComponent>
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedScaleVisualsSystem _scale = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IgnoreSpiderWebComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<IgnoreSpiderWebComponent> ent, ref MapInitEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var comp, out _))
        {
            if (!_whitelist.CheckBoth(ent, comp.Blacklist, comp.Whitelist))
                continue;

            Grow(ent, comp);
        }
    }

    private void Grow(EntityUid uid, GiantSpidersRuleComponent comp)
    {
        _scale.SetSpriteScale(uid, _scale.GetSpriteScale(uid) * comp.Scale);
        _physics.ScaleFixtures(uid, comp.Scale);

        if (TryComp<MobThresholdsComponent>(uid, out var thresholds))
        {
            foreach (var (damage, state) in thresholds.Thresholds.Reverse().ToList())
            {
                _thresholds.SetMobStateThreshold(uid, damage * comp.Health, state, thresholds);
            }
        }

        if (TryComp<MeleeWeaponComponent>(uid, out var melee))
        {
            melee.Damage *= comp.Damage;
            Dirty(uid, melee);
        }
    }
}
