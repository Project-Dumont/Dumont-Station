using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

public sealed class TemperatureExplosionSystem : EntitySystem
{
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemperatureExplosionComponent, AtmosExposedUpdateEvent>(OnAtmosUpdate);
    }

    private void OnAtmosUpdate(
        EntityUid uid,
        TemperatureExplosionComponent component,
        ref AtmosExposedUpdateEvent args)
    {
        var airTemperature = args.GasMixture.Temperature;
        if (component.HeatDamage.DamageDict.Count > 0 &&
            airTemperature >= component.HeatDamageTemperature)
        {
            _popup.PopupEntity(Loc.GetString("item-temperature-overheat", ("item", uid)), uid);
            _damageableSystem.TryChangeDamage(uid, component.HeatDamage);
        }
        if (airTemperature >= component.ExplosionTemperature)
        {
            _explosionSystem.TriggerExplosive(uid, radius: component.ExplosionRadius);
        }
    }
}