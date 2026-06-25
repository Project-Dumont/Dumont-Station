// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Trauma.Nuclear.Monitor;
using Content.Shared._Trauma.Nuclear.Turbine;

namespace Content.Client._Trauma.Nuclear.Turbine.UI;

/// <summary>
/// Initializes a <see cref="TurbineWindow"/>.
/// </summary>
public sealed partial class TurbineBUI(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private TurbineWindow? _window;

    protected override void Open()
    {
        if (GetTurbine(Owner, out var monitor) is not { } turbine)
            return;

        base.Open();

        _window = this.CreateWindow<TurbineWindow>();
        _window.SetEntity(turbine, monitor);

        _window.OnChangeFlowRate += val => SendPredictedMessage(new TurbineChangeFlowRateMessage(val));
        _window.OnChangeStatorLoad += val => SendPredictedMessage(new TurbineChangeStatorLoadMessage(val));
    }

    private Entity<TurbineComponent>? GetTurbine(EntityUid uid, out EntityUid? monitor)
    {
        monitor = null;
        if (EntMan.TryGetComponent<TurbineComponent>(uid, out var turbine))
            return (uid, turbine);

        if (EntMan.GetComponent<NuclearMonitorComponent>(uid).Linked is { } linked)
        {
            monitor = uid;
            return (linked, EntMan.GetComponent<TurbineComponent>(linked));
        }

        return null;
    }
}
