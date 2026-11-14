// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
// Dumont start
using Content.Shared.Damage;
// Dumont end

using BloodstreamSystem = Content.Shared.Body.Systems.SharedBloodstreamSystem;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Goobstation.Common.Weapons.DelayedKnockdown;
using Content.Goobstation.Shared.Disease.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Stunnable;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mind.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Temperature.Systems;
using Content.Trauma.Server.Heretic.Abilities;
using Content.Trauma.Shared.Heretic.Components;
using Content.Trauma.Shared.Heretic.Components.Ghoul;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;
using Content.Trauma.Shared.Heretic.Systems.Abilities;
using Robust.Shared.Timing;
// Dumont end

// Dumont start
using BodyRestoreSystem = Content.Shared.Body.Systems.SharedBodySystem;
// Dumont end

namespace Content.Trauma.Server.Heretic.Systems.PathSpecific;

public sealed partial class LeechingWalkSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private HereticAbilitySystem _ability = default!;
    [Dependency] private RespiratorSystem _respirator = default!;
    [Dependency] private DamageableSystem _dmg = default!;
    [Dependency] private BodyRestoreSystem _bodyRestore = default!;
    [Dependency] private BloodstreamSystem _blood = default!;
    [Dependency] private Content.Server.Temperature.Systems.TemperatureSystem _temp = default!;
    [Dependency] private SharedStaminaSystem _stam = default!;
    [Dependency] private StunSystem _stun = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private EntityQuery<DamageableComponent> _damageableQuery = default!;
    [Dependency] private EntityQuery<StaminaComponent> _staminaQuery = default!;
    [Dependency] private EntityQuery<RespiratorComponent> _respiratorQuery = default!;
    [Dependency] private EntityQuery<HereticComponent> _hereticQuery = default!;
    [Dependency] private EntityQuery<GhoulComponent> _ghoulQuery = default!;
    [Dependency] private EntityQuery<BodyComponent> _bodyQuery = default!;
    [Dependency] private EntityQuery<BloodstreamComponent> _bloodQuery = default!;

    private static readonly TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);
    private TimeSpan _nextUpdate = TimeSpan.Zero;

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<LeechingWalkComponent> ent, ref MapInitEvent args)
    {
        RemCompDeferred<DiseaseCarrierComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        if (_nextUpdate > now)
            return;

        _nextUpdate = now + UpdateDelay;

        var leechQuery = EntityQueryEnumerator<LeechingWalkComponent, MindContainerComponent, TransformComponent>();
        while (leechQuery.MoveNext(out var uid, out var leech, out var mindContainer, out var xform))
        {
            if (!_ability.IsTileRust(xform.Coordinates, out _))
                continue;

            _damageableQuery.TryComp(uid, out var damageable);

            var multiplier = 1f;
            var shouldHeal = true;
            var boneHeal = FixedPoint2.Zero;
            if (_hereticQuery.TryComp(mindContainer.Mind, out var heretic) && heretic.CurrentPath == HereticPath.Rust)
            {
                multiplier += heretic.PassiveLevel * 0.5f;
                if (heretic.Ascended)
                {
                    if (_respiratorQuery.TryComp(uid, out var respirator))
                    {
                        _respirator.UpdateSaturation(uid,
                            respirator.MaxSaturation - respirator.MinSaturation,
                            respirator);
                    }

                    multiplier += 1.5f;
                }

                if (heretic.PassiveLevel >= 2)
                    boneHeal = leech.BoneHeal * heretic.PassiveLevel;

                if (heretic.PassiveLevel >= 3)
                {
                    if (damageable != null && _dmg.GetAllDamage(uid).GetTotal() < FixedPoint2.Epsilon)
                    {
                        if (_bodyQuery.TryComp(uid, out var body))
                            _bodyRestore.RestoreBody((uid, body));
                        shouldHeal = false;
                    }
                }
            }
            else if (_ghoulQuery.HasComp(uid))
                multiplier = 2f;

            RemCompDeferred<DelayedKnockdownComponent>(uid);

            var toHeal = -SharedHereticAbilitySystem.AllDamage * multiplier;

            if (shouldHeal && damageable != null)
            {
                _ability.IHateWoundMed((uid, damageable, null),
                    toHeal,
                    leech.BloodHeal * multiplier,
                    null,
                    boneHeal);
            }

            if (_bloodQuery.TryComp(uid, out var blood))
                _blood.FlushChemicals((uid, blood), leech.ChemPurgeRate * multiplier, leech.ExcludedReagents);

            _temp.ForceChangeTemperature(uid, leech.TargetTemperature);

            if (_staminaQuery.TryComp(uid, out var stamina) && stamina.StaminaDamage > 0)
            {
                _stam.TakeStaminaDamage(uid,
                    -float.Min(leech.StaminaHeal * multiplier, stamina.StaminaDamage),
                    stamina,
                    visual: false);
            }

            var reduction = leech.StunReduction * multiplier;
            _stun.TryAddStunDuration(uid, -reduction);
            _stun.AddKnockdownTime(uid, -reduction);

            foreach (var id in leech.RemovedStatusEffects)
            {
                _status.TryRemoveStatusEffect(uid, id);
            }
        }
    }
}
