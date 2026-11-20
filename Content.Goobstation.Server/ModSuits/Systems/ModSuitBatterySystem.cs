using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared.PowerCell.Components;

namespace Content.Goobstation.Server.ModSuits.Systems;

public sealed class ModSuitBatterySystem : EntitySystem
{
    [Dependency] private BatterySystem _battery = default!;
    [Dependency] private PowerCellSystem _powerCell = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private ModCoreSystem _modCore = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModCoreComponent, ModSuitBatteryInfoEvent>(OnBatteryInfo);
        SubscribeLocalEvent<ModCoreComponent, ModSuitBatteryUseEvent>(OnBatteryUse);
        SubscribeLocalEvent<ModCoreComponent, ModSuitBatteryChangeEvent>(OnBatteryChange);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (frameTime <= 0f)
            return;

        var coreQuery = EntityQueryEnumerator<ModCoreComponent>();
        while (coreQuery.MoveNext(out var coreUid, out var coreComp))
        {
            if (!TryGetBattery(coreUid, out var batteryUid, out _))
            {
                _modCore.UpdateBatteryInfo((coreUid, coreComp), false, 0f, 0f);
                continue;
            }

            var charge = GetChargeInfo(batteryUid);
            _modCore.UpdateBatteryInfo((coreUid, coreComp), true, charge.CurrentCharge, charge.MaxCharge);
        }

        var query = EntityQueryEnumerator<ModSuitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var suit = (uid, comp);
            if (!_modSuit.IsPowerDrainEnabled(suit))
                continue;

            if (_modSuit.GetCore(suit) is not { } core
                || !TryGetBattery(core, out var batteryUid, out var battery))
            {
                _modSuit.SetPowered(suit, false);
                continue;
            }

            var drain = _modSuit.GetTotalDrain(suit);
            if (drain > 0f)
                _battery.UseCharge(batteryUid, drain * frameTime, battery);

            var charge = GetChargeInfo(batteryUid);
            _modSuit.SetPowered(suit, charge.CurrentCharge > 0f);
        }
    }

    private void OnBatteryInfo(Entity<ModCoreComponent> ent, ref ModSuitBatteryInfoEvent args)
    {
        if (!TryGetBattery(ent.Owner, out var batteryUid, out _))
            return;

        var charge = GetChargeInfo(batteryUid);
        _modCore.UpdateBatteryInfo(ent, true, charge.CurrentCharge, charge.MaxCharge);
        args.Found = true;
        args.Charge = charge.CurrentCharge;
        args.MaxCharge = charge.MaxCharge;
    }

    private void OnBatteryUse(Entity<ModCoreComponent> ent, ref ModSuitBatteryUseEvent args)
    {
        if (!TryGetBattery(ent.Owner, out var batteryUid, out var battery))
            return;

        args.Succeeded = _battery.TryUseCharge(batteryUid, args.Amount, battery);
    }

    private void OnBatteryChange(Entity<ModCoreComponent> ent, ref ModSuitBatteryChangeEvent args)
    {
        if (!TryGetBattery(ent.Owner, out var batteryUid, out var battery))
            return;

        _battery.ChangeCharge(batteryUid, args.Amount, battery);
    }

    private bool TryGetBattery(EntityUid core, out EntityUid batteryUid, out BatteryComponent? battery)
    {
        if (TryComp(core, out battery))
        {
            batteryUid = core;
            return true;
        }

        if (_powerCell.TryGetBatteryFromSlot(core, out var slotBatteryUid, out battery)
            && slotBatteryUid is { } uid)
        {
            batteryUid = uid;
            return true;
        }

        batteryUid = default;
        battery = null;
        return false;
    }

    private GetChargeEvent GetChargeInfo(EntityUid batteryUid)
    {
        var charge = new GetChargeEvent();
        RaiseLocalEvent(batteryUid, ref charge);
        return charge;
    }
}
