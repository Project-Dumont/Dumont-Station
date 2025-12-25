using Content.Goobstation.Common.Effects;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._Orion.Power.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server._Orion.Power.Systems;

public sealed class InducerSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SparksSystem _sparks = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InducerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<InducerComponent, InducerDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<InducerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnAfterInteract(EntityUid uid, InducerComponent component, AfterInteractEvent args)
    {
        if (args.Handled || args.Target == null || !args.CanReach)
            return;

        var target = args.Target.Value;

        if (!TryComp<BatteryComponent>(target, out var targetBattery))
        {
            _popup.PopupEntity(Loc.GetString("inducer-no-battery"), uid, args.User);
            return;
        }

        if (!_itemSlots.TryGetSlot(uid, component.PowerCellSlotId, out var slot) || slot.Item == null || !TryComp<BatteryComponent>(slot.Item.Value, out var sourceBattery))
        {
            _popup.PopupEntity(Loc.GetString("inducer-no-power-cell"), uid, args.User);
            return;
        }

        if (sourceBattery.CurrentCharge <= 0)
        {
            _popup.PopupEntity(Loc.GetString("inducer-empty"), uid, args.User);
            return;
        }

        if (_battery.IsFull(target, targetBattery))
        {
            _popup.PopupEntity(Loc.GetString("inducer-target-full"), uid, args.User);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, component.TransferDelay, new InducerDoAfterEvent(), uid, target: target, used: uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            RequireCanInteract = true,
            DistanceThreshold = component.MaxDistance,
            CancelDuplicate = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, InducerComponent component, InducerDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        var target = args.Target.Value;

        if (!TryComp<BatteryComponent>(target, out var targetBattery))
            return;

        if (!_itemSlots.TryGetSlot(uid, component.PowerCellSlotId, out var slot) || slot.Item == null)
            return;

        if (!TryComp<BatteryComponent>(slot.Item.Value, out var sourceBattery))
            return;

        var baseEnergyToConsume = component.TransferRate * component.TransferDelay;
        baseEnergyToConsume = Math.Min(baseEnergyToConsume, sourceBattery.CurrentCharge);

        if (baseEnergyToConsume <= 0)
            return;

        var energyToReceive = baseEnergyToConsume * component.TransferMultiplier;
        var freeSpace = targetBattery.MaxCharge - targetBattery.CurrentCharge;
        energyToReceive = Math.Min(energyToReceive, freeSpace);

        var actualEnergyToConsume = energyToReceive / component.TransferMultiplier;
        if (_battery.TryUseCharge(slot.Item.Value, actualEnergyToConsume, sourceBattery))
        {
            _battery.AddCharge(target, energyToReceive, targetBattery);
            _sparks.DoSparks(Transform(target).Coordinates);

            args.Repeat = targetBattery.CurrentCharge < targetBattery.MaxCharge;
        }
        else
        {
            _battery.SetCharge(target, targetBattery.CurrentCharge + energyToReceive, targetBattery);
            _battery.SetCharge(slot.Item.Value, sourceBattery.CurrentCharge - actualEnergyToConsume, sourceBattery);
            args.Repeat = false;
        }
    }

    private void OnGetVerbs(EntityUid uid, InducerComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var priority = 0;
        foreach (var rate in component.AvailableTransferRates)
        {
            AlternativeVerb verb = new()
            {
                Text = Loc.GetString("inducer-set-transfer-rate", ("rate", rate)),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/zap.svg.192dpi.png")),
                Category = VerbCategory.SelectType,
                Act = () =>
                {
                    if (Math.Abs(component.TransferRate - rate) < 0.01f)
                        return;

                    component.TransferRate = rate;
                    Dirty(uid, component);
                    _popup.PopupEntity(Loc.GetString("inducer-transfer-rate-set", ("rate", rate)), uid, args.User);
                },

                Priority = priority,
            };

            priority--;

            args.Verbs.Add(verb);
        }
    }
}
