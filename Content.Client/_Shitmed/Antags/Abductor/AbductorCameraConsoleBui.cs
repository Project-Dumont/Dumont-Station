// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Antags.Abductor;
using Content.Client._Shitmed.Choice.UI;
using JetBrains.Annotations;
using static Content.Shared.Pinpointer.SharedNavMapSystem;

namespace Content.Client._Shitmed.Antags.Abductor;

[UsedImplicitly]
public sealed class AbductorCameraConsoleBui : BoundUserInterface
{
    [ViewVariables]
    private AbductorCameraConsoleWindow? _window;

    private NetEntity? _station;

    public AbductorCameraConsoleBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
    protected override void Open()
    {
        base.Open();

        _window = new AbductorCameraConsoleWindow();

        _window.OnClose += Close;
        _window.Title = "Intercepted cameras.";

        _window.StationsButton.OnPressed += _ =>
        {
            _station = null;
            View(ViewType.Stations);
        };
    }

    protected override void UpdateState(BoundUserInterfaceState? state)
    {
        if (state is AbductorCameraConsoleBuiState s)
            Update(s);
    }

    private void Update(AbductorCameraConsoleBuiState state)
    {
        View(ViewType.Stations);

        RefreshUI(state);

        if (!_window!.IsOpen)
            _window.OpenCentered();
    }

    private void OnStationPressed(NetEntity station, List<NavMapBeacon> beacons)
    {
        if (_window == null)
            return;

        _station = station;

        foreach (var beacon in beacons)
        {
            var beaconButton = new ChoiceControl();

            beaconButton.Set(beacon.Text, null);
            beaconButton.Button.Modulate = beacon.Color;
            beaconButton.Button.OnPressed += _ =>
            {
                SendMessage(new AbductorBeaconChosenBuiMsg()
                {
                    Beacon = beacon,
                });
                Close();
            };
            _window.Beacons.AddChild(beaconButton);
        }
        View(ViewType.Beacons);
    }

    private void RefreshUI(AbductorCameraConsoleBuiState state)
    {
        if (_window == null)
            return;

        _window!.Stations.DisposeAllChildren();
        _window.Beacons.DisposeAllChildren();

        foreach (var station in state.Stations)
        {
            var stationButton = new ChoiceControl();

            var tooltip = station.Value.IsEnabled ? "" : Loc.GetString("abductors-ui-out-of-range");

            stationButton.Set(station.Value.Name, station.Value.IsEnabled, tooltip);
            stationButton.Button.OnPressed += _ => OnStationPressed(station.Key, station.Value.Beacons);

            _window.Stations.AddChild(stationButton);

            if (station.Key == _station && station.Value.IsEnabled) OnStationPressed(station.Key, station.Value.Beacons);
        }
    }

    private void View(ViewType type)
    {
        if (_window == null)
            return;

        _window.StationsButton.Parent!.Margin = new Thickness(0, 0, 0, 10);

        _window.Stations.Visible = type == ViewType.Stations;
        _window.StationsButton.Visible = true;

        _window.Beacons.Visible = type == ViewType.Beacons;
        _window.BeaconsButton.Disabled = type != ViewType.Beacons;

        _window.Title = State is not AbductorCameraConsoleBuiState state
            || _station == null
            || !state.Stations.TryGetValue(_station.Value, out var station)
            ? "Stations"
            : $"Station - {station.Name}";
    }

    private enum ViewType
    {
        Stations,
        Beacons,
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window?.Dispose();
    }
}