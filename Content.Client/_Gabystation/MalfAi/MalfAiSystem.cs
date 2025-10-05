using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Shared._Gabystation.MalfAi;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared.Alert.Components;
using Content.Shared.MalfAI.Components;
using Content.Shared.Store.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Gabystation.MalfAi;

public sealed class MalfAiSystem : SharedMalfAiSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly ClientAlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        // SubscribeLocalEvent<MalfunctioningAiComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
        SubscribeLocalEvent<MalfunctioningAiComponent, GetGenericAlertCounterAmountEvent>(OnAlertCounter);
    }

    private void OnAlertCounter(Entity<MalfunctioningAiComponent> malf, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Alert.ID != malf.Comp.CurrencyAlertId)
            return;

        args.Amount = (int) malf.Comp.CpuStore;
    }

    // public void OnUpdateAlert(Entity<MalfunctioningAiComponent> malf, ref UpdateAlertSpriteEvent args)
    // {
    //     if (args.Alert.ID != malf.Comp.CurrencyAlertId)
    //         return;

    //     var cpu = Math.Clamp((int) malf.Comp.CpuStore, 0, 999);
    //     var alert = args.SpriteViewEnt;

    //     _sprite.LayerSetRsiState((alert.Owner, alert.Comp), CPUAlertVisualLayers.Digit1, $"{cpu / 100 % 10}");
    //     _sprite.LayerSetRsiState((alert.Owner, alert.Comp), CPUAlertVisualLayers.Digit2, $"{cpu / 10 % 10}");
    //     _sprite.LayerSetRsiState((alert.Owner, alert.Comp), CPUAlertVisualLayers.Digit3, $"{cpu % 10}");
    // }
}