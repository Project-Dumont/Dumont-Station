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
    [Dependency] private PowerReceiverSystem _powerReceiver = default!;

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

        if (component.HeatDamageTemperature is { } heatDamageTemperature &&
        airTemperature >= heatDamageTemperature &&
        component.Damage.DamageDict.Count > 0)
        {
            _popup.PopupEntity(Loc.GetString("item-temperature-overheat", ("item", uid)), uid);
            _damageableSystem.TryChangeDamage(uid, component.Damage);
        }
        if (component.ExplosionTemperature is { } explosionTemperature &&
        airTemperature >= explosionTemperature)
        {
            _explosionSystem.TriggerExplosive(uid, radius: component.ExplosionRadius);
        }
        
        UpdateShutdown(uid, component, airTemperature);
    }
    
    private void UpdateShutdown(
    EntityUid uid,
    OverheatMachineComponent component,
    float airTemperature)
    {
        if (component.TemperatureShutdown is not { } temperatureShutdown)
            return; 
        if (component.IsShutdown == (airTemperature >= temperatureShutdown))
            return;
        component.IsShutdown = airTemperature >= temperatureShutdown;

        UpdatePowerReceiver(uid, component.IsShutdown);
        UpdateDamageOnInteract(uid, component.IsShutdown);
    }

    private void UpdatePowerReceiver(EntityUid uid, bool shutdown)
    {
        if (!TryComp<ApcPowerReceiverComponent>(uid, out var power))
            return;

        _powerReceiver.SetPowerDisabled(uid, shutdown, power);
    }

    private void UpdateDamageOnInteract(EntityUid uid, bool shutdown)
    {
        if (!TryComp<DamageOnInteractComponent>(uid, out var damageOnInteractComp))
            return;

        _damageOnInteractSystem.SetIsDamageActiveTo(
        (uid, damageOnInteractComp),
        shutdown);
    }

    private void OnExamined(
    EntityUid uid,
    OverheatMachineComponent component,
    ExaminedEvent args)
    {
        if (!component.IsShutdown)
            return;
            
        args.PushMarkup(Loc.GetString("item-temperature-shutdown"));
    }

   private void OnTryTogglePower(
   EntityUid uid,
   OverheatMachineComponent component,
   ref TryTogglePowerEvent args)
    {
        if (!component.IsShutdown)
            return;

        args.Cancel();
    }
}