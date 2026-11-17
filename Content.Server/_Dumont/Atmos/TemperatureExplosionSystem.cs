using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Shared.Damage;

public sealed partial class OverheatMachineSystem : EntitySystem
{
    [Dependency] private ExplosionSystem _explosionSystem = default!;
    [Dependency] private DamageableSystem _damageableSystem = default!;
    [Dependency] private PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OverheatMachineComponent, AtmosExposedUpdateEvent>(OnAtmosUpdate);
    }

    private void OnAtmosUpdate(
        EntityUid uid,
        OverheatMachineComponent component,
        ref AtmosExposedUpdateEvent args)
    {
        var airTemperature = args.GasMixture.Temperature;
        if (component.HeatDamageTemperature is not null &&
            airTemperature >= component.HeatDamageTemperature &&
            component.HeatDamage.DamageDict.Count > 0)
        {
            _popup.PopupEntity(Loc.GetString("item-temperature-overheat", ("item", uid)), uid);
            _damageableSystem.TryChangeDamage(uid, component.HeatDamage);
        }
        if (component.ExplosionTemperature is not null &&
            airTemperature >= component.ExplosionTemperature)
        {
            _explosionSystem.TriggerExplosive(uid, radius: component.ExplosionRadius);
        }
    }
}