using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.PowerCell.Components;
using Content.Shared.Popups;
using Content.Shared.Rounding;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModSuitSystem
{
    private static readonly TimeSpan PowerAlertInterval = TimeSpan.FromSeconds(1);
    private TimeSpan _nextPowerAlertUpdate;

    public EntityUid? GetCore(Entity<ModSuitComponent> ent)
    {
        return _itemSlots.GetItemOrNull(ent, ent.Comp.CoreSlotId);
    }

    private bool TryGetBatteryInfo(Entity<ModSuitComponent> ent, out ModSuitBatteryInfoEvent info)
    {
        info = default;
        if (GetCore(ent) is not { } core || !HasComp<ModCoreComponent>(core))
            return false;

        if (_net.IsClient && TryComp<ModCoreComponent>(core, out var coreComp))
        {
            info.Found = coreComp.HasBattery;
            info.Charge = coreComp.Charge;
            info.MaxCharge = coreComp.MaxCharge;
            return info.Found;
        }

        RaiseLocalEvent(core, ref info);
        return info.Found;
    }

    public bool HasCharge(Entity<ModSuitComponent> ent, float amount)
    {
        return TryGetBatteryInfo(ent, out var battery) && battery.Charge >= amount;
    }

    public bool TryUseCharge(Entity<ModSuitComponent> ent, float amount)
    {
        if (amount <= 0f)
            return true;

        if (GetCore(ent) is not { } core || !HasComp<ModCoreComponent>(core))
            return false;

        var ev = new ModSuitBatteryUseEvent { Amount = amount };
        RaiseLocalEvent(core, ref ev);
        return ev.Succeeded;
    }

    public void ChangeCharge(Entity<ModSuitComponent> ent, float amount)
    {
        if (GetCore(ent) is not { } core || !HasComp<ModCoreComponent>(core))
            return;

        var ev = new ModSuitBatteryChangeEvent { Amount = amount };
        RaiseLocalEvent(core, ref ev);
    }

    public float GetThemeDrain(Entity<ModSuitComponent> ent)
    {
        return _proto.Index(ent.Comp.Theme).ChargeDrain;
    }

    public float GetTotalDrain(Entity<ModSuitComponent> ent)
    {
        if (ent.Comp.State == ModSuitState.Inactive)
            return 0f;

        var drain = GetThemeDrain(ent);
        if (ent.Comp.State != ModSuitState.Active)
            return drain;

        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (!TryComp<ModModuleComponent>(module, out var moduleComp))
                continue;

            drain += moduleComp.IdlePower;
            if (moduleComp.Active)
                drain += moduleComp.ActivePower;
        }

        return drain;
    }

    [SubscribeLocalEvent]
    private void OnPowerCellChanged(Entity<ModSuitComponent> ent, ref PowerCellChangedEvent args)
    {
        if (_timing.ApplyingState || ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        RaiseUpdated(ent);
        SetPowered(ent, !args.Ejected && HasCharge(ent, 0.01f));
        UpdatePowerAlert(ent);
    }

    [SubscribeLocalEvent]
    private void OnSuitEntInserted(Entity<ModSuitComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (ent.Comp.LifeStage < ComponentLifeStage.Running)
            return;

        RaiseUpdated(ent);

        if (args.Container.ID != ent.Comp.CoreSlotId || _timing.ApplyingState)
            return;

        SetPowered(ent, HasCharge(ent, 0.01f));
    }

    [SubscribeLocalEvent]
    private void OnSuitEntRemoved(Entity<ModSuitComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (ent.Comp.LifeStage < ComponentLifeStage.Running)
            return;

        RaiseUpdated(ent);

        if (args.Container.ID != ent.Comp.CoreSlotId || _timing.ApplyingState)
            return;

        SetPowered(ent, false);
    }

    private void RaiseUpdated(EntityUid suit)
    {
        var ev = new ModSuitUpdatedEvent();
        RaiseLocalEvent(suit, ref ev);
    }

    public bool IsPowerDrainEnabled(Entity<ModSuitComponent> ent)
    {
        return ent.Comp.State != ModSuitState.Inactive;
    }

    public void SetPowered(Entity<ModSuitComponent> ent, bool powered)
    {
        if (_net.IsClient)
            return;

        if (ent.Comp.Powered == powered)
            return;

        ent.Comp.Powered = powered;
        Dirty(ent);

        if (ent.Comp.State == ModSuitState.Active)
        {
            if (powered)
            {
                ActivatePassiveModules(ent);
            }
            else
            {
                DeactivateAllModules(ent, true);
                if (ent.Comp.Wearer is { } wearer)
                    _popup.PopupClient(Loc.GetString("modsuit-out-of-power"), wearer, wearer, PopupType.MediumCaution);
            }
        }

        RefreshSpeed(ent);
        UpdatePowerAlert(ent);
    }

    private void UpdatePowerAlerts()
    {
        if (_net.IsClient)
            return;

        if (_timing.CurTime < _nextPowerAlertUpdate)
            return;

        _nextPowerAlertUpdate = _timing.CurTime + PowerAlertInterval;

        var query = EntityQueryEnumerator<ModSuitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Wearer != null && comp.State != ModSuitState.Inactive && ShouldStep(comp))
                UpdatePowerAlert((uid, comp));
        }
    }

    private void UpdatePowerAlert(Entity<ModSuitComponent> ent)
    {
        if (_timing.ApplyingState || ent.Comp.Wearer is not { } wearer)
            return;

        if (ent.Comp.State == ModSuitState.Inactive || !TryGetBatteryInfo(ent, out var battery))
        {
            _alerts.ClearAlert(wearer, ent.Comp.PowerAlert);
            return;
        }

        var severity = ContentHelpers.RoundToLevels(MathF.Max(0f, battery.Charge), battery.MaxCharge, 6);
        _alerts.ShowAlert(wearer, ent.Comp.PowerAlert, (short) severity);
    }
}
