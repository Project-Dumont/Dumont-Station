using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Server.Power.Components; 
using Content.Server.Power.EntitySystems;
using Content.Server.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared._Dumont.Power.Events;
using Content.Shared.Damage.Components;

namespace Content.Server.Temperature.EntitySystems;

public sealed partial class OverheatMachineSystem : EntitySystem
{
    [Dependency] private ExplosionSystem _explosionSystem = default!;
    [Dependency] private DamageableSystem _damageableSystem = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private DamageOnInteractSystem _damageOnInteractSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OverheatMachineComponent, AtmosExposedUpdateEvent>(OnAtmosUpdate);
        SubscribeLocalEvent<OverheatMachineComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<OverheatMachineComponent, TryTogglePowerEvent>(OnTryTogglePower);
    }

    private void OnAtmosUpdate(
        EntityUid uid,
        OverheatMachineComponent component,
        ref AtmosExposedUpdateEvent args)
    {
        var airTemperature = args.GasMixture.Temperature;
        if (component.HeatDamageTemperature is not null &&
            airTemperature >= component.HeatDamageTemperature &&
            component.Damage.DamageDict.Count > 0)
        {
            _popup.PopupEntity(Loc.GetString("item-temperature-overheat", ("item", uid)), uid);
            _damageableSystem.TryChangeDamage(uid, component.Damage);
        }
        if (component.ExplosionTemperature is not null &&
            airTemperature >= component.ExplosionTemperature)
        {
            _explosionSystem.TriggerExplosive(uid, radius: component.ExplosionRadius);
        }
        if (component.TemperatureShutdown is not null)
        {
            var shutdown = airTemperature >= component.TemperatureShutdown;
            if (component.IsShutdown != shutdown)
            {
                component.IsShutdown = shutdown;
                if (TryComp<ApcPowerReceiverComponent>(uid, out var power))
                {
                    var powerManager = EntityManager.System<PowerReceiverSystem>();
                    powerManager.SetPowerDisabled(uid, shutdown, power);
                }
                if (TryComp<DamageOnInteractComponent>(uid, out var damageOnInteractComp))
                    _damageOnInteractSystem.SetIsDamageActiveTo((uid, damageOnInteractComp), shutdown);
            }
        }

    }

    private void OnExamined(EntityUid uid, 
    OverheatMachineComponent component, 
    ExaminedEvent args)
    {
        if (component.IsShutdown)
            args.PushMarkup(Loc.GetString("item-temperature-shutdown"));
    }

    private void OnTryTogglePower(
    EntityUid uid,
    OverheatMachineComponent component,
    ref TryTogglePowerEvent args)
    {
        if (component.IsShutdown)
            args.Cancel();
    }
}