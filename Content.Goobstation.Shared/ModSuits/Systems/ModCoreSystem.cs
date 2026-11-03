using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Emp;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModCoreSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStackSystem _stack = default!;

    public bool HasBattery(Entity<ModCoreComponent> ent)
    {
        return ent.Comp.HasBattery;
    }

    public float GetChargeLevel(Entity<ModCoreComponent> ent)
    {
        return ent.Comp.MaxCharge > 0f ? ent.Comp.Charge / ent.Comp.MaxCharge : 0f;
    }

    public void UpdateBatteryInfo(Entity<ModCoreComponent> ent, bool hasBattery, float charge, float maxCharge)
    {
        if (ent.Comp.HasBattery == hasBattery
            && ent.Comp.Charge == charge
            && ent.Comp.MaxCharge == maxCharge)
            return;

        ent.Comp.HasBattery = hasBattery;
        ent.Comp.Charge = charge;
        ent.Comp.MaxCharge = maxCharge;
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnEmpAttempt(Entity<ModCoreComponent> ent, ref EmpAttemptEvent args)
    {
        RelayToSuit(ent, ref args);
    }

    private void RelayToSuit<T>(Entity<ModCoreComponent> ent, ref T args) where T : notnull
    {
        if (_container.TryGetContainingContainer((ent.Owner, null, null), out var container)
            && HasComp<ModSuitComponent>(container.Owner))
            RaiseLocalEvent(container.Owner, ref args);
    }

    [SubscribeLocalEvent]
    private void OnRefuel(Entity<ModCoreRefuelableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !_whitelist.IsValid(ent.Comp.Fuel, args.Used))
            return;

        args.Handled = true;

        var battery = new ModSuitBatteryInfoEvent();
        RaiseLocalEvent(ent.Owner, ref battery);
        if (!battery.Found)
            return;

        var missing = battery.MaxCharge - battery.Charge;
        if (missing <= 0f)
        {
            _popup.PopupClient(Loc.GetString("modsuit-core-full"), args.Target, args.User);
            return;
        }

        var wanted = Math.Max(1, (int) MathF.Ceiling(missing / ent.Comp.ChargePerFuel));
        var used = 1;
        if (TryComp<StackComponent>(args.Used, out var stack))
        {
            used = Math.Min(wanted, stack.Count);
            _stack.SetCount(args.Used, stack.Count - used, stack);
        }
        else
        {
            PredictedQueueDel(args.Used);
        }

        var change = new ModSuitBatteryChangeEvent { Amount = used * ent.Comp.ChargePerFuel };
        RaiseLocalEvent(ent.Owner, ref change);
        _audio.PlayPredicted(ent.Comp.RefuelSound, args.Target, args.User);
        _popup.PopupClient(Loc.GetString("modsuit-core-refueled"), args.Target, args.User);
    }
}
