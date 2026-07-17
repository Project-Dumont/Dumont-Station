// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._Starlight.MassDriver;
using Content.Shared._Starlight.MassDriver.Components;
using Content.Shared._Starlight.MassDriver.EntitySystems;
using Content.Shared.DeviceLinking.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Server._Starlight.MassDriver.EntitySystems;

public sealed partial class MassDriverSystem : SharedMassDriverSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiver = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MassDriverComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<MassDriverConsoleComponent, MassDriverModeMessage>(OnModeChanged);
        SubscribeLocalEvent<MassDriverConsoleComponent, MassDriverLaunchMessage>(OnLaunch);
        SubscribeLocalEvent<MassDriverConsoleComponent, MassDriverThrowSpeedMessage>(OnThrowSpeedChanged);
        SubscribeLocalEvent<MassDriverConsoleComponent, MassDriverThrowDistanceMessage>(OnThrowDistanceChanged);
        SubscribeLocalEvent<MassDriverConsoleComponent, BoundUIOpenedEvent>(OnUIOpen);

        SubscribeLocalEvent<MassDriverConsoleComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<MassDriverConsoleComponent, PortDisconnectedEvent>(OnPortDisconnected);
        SubscribeLocalEvent<MassDriverComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnNewLink(EntityUid uid, MassDriverConsoleComponent component, NewLinkEvent args)
    {
        if (args.SourcePort != component.LinkingPort ||
            !TryComp<MassDriverComponent>(args.Sink, out var driver))
        {
            return;
        }

        if (!component.MassDrivers.Contains(args.Sink))
            component.MassDrivers.Add(args.Sink);

        driver.Console = uid;
        Dirty(args.Sink, driver);
        Dirty(uid, component);
        SendState(uid, component);
    }

    private void OnPortDisconnected(EntityUid uid, MassDriverConsoleComponent component, PortDisconnectedEvent args)
    {
        var massDriverUid = args.RemovedPortUid;
        if (args.Port != component.LinkingPort || !component.MassDrivers.Contains(massDriverUid))
            return;

        if (TryComp<MassDriverComponent>(massDriverUid, out var driver) && driver.Console == uid)
        {
            driver.Console = null;
            Dirty(massDriverUid, driver);
        }

        component.MassDrivers.Remove(massDriverUid);
        Dirty(uid, component);
        SendState(uid, component);
    }

    private void OnSignalReceived(EntityUid uid, MassDriverComponent component, ref SignalReceivedEvent args)
    {
        if (args.Port == component.LaunchPort && component.Mode == MassDriverMode.Manual && _powerReceiver.IsPowered(uid))
            StartManualLaunch(uid, component);
    }

    public override void ChangePowerLoad(EntityUid uid, MassDriverComponent component, float powerLoad)
    {
        if (TryComp<ApcPowerReceiverComponent>(uid, out var receiver))
            _powerReceiver.SetLoad(receiver, powerLoad);
    }

    private void OnGetState(EntityUid uid, MassDriverComponent component, ref ComponentGetState args)
    {
        args.State = CreateState(component);
    }

    private void OnModeChanged(EntityUid uid, MassDriverConsoleComponent component, MassDriverModeMessage args)
    {
        foreach (var massDriverUid in component.MassDrivers)
        {
            if (!TryComp<MassDriverComponent>(massDriverUid, out var massDriverComponent))
                continue;

            massDriverComponent.Mode = args.Mode;
            Dirty(massDriverUid, massDriverComponent);

            if (massDriverComponent.Mode == MassDriverMode.Auto)
            {
                EnsureComp<ActiveMassDriverComponent>(massDriverUid);
            }
            else
            {
                if (TryComp<ActiveMassDriverComponent>(massDriverUid, out var activeMassDriver))
                    StopLaunching(massDriverUid, massDriverComponent, activeMassDriver, true);

                RemComp<ActiveMassDriverComponent>(massDriverUid);
            }
        }

        SendState(uid, component);
    }

    private void OnLaunch(EntityUid uid, MassDriverConsoleComponent component, MassDriverLaunchMessage args)
    {
        foreach (var massDriverUid in component.MassDrivers)
        {
            if (!TryComp<MassDriverComponent>(massDriverUid, out var massDriverComponent) ||
                massDriverComponent.Mode != MassDriverMode.Manual ||
                !_powerReceiver.IsPowered(massDriverUid))
            {
                continue;
            }

            StartManualLaunch(massDriverUid, massDriverComponent);
        }
    }

    private void OnThrowSpeedChanged(EntityUid uid, MassDriverConsoleComponent component, MassDriverThrowSpeedMessage args)
    {
        foreach (var massDriverUid in component.MassDrivers)
        {
            if (!TryComp<MassDriverComponent>(massDriverUid, out var massDriverComponent))
                continue;

            massDriverComponent.CurrentThrowSpeed = Math.Clamp(
                args.Speed,
                massDriverComponent.MinThrowSpeed,
                massDriverComponent.MaxThrowSpeed);
            Dirty(massDriverUid, massDriverComponent);
        }

        SendState(uid, component);
    }

    private void OnThrowDistanceChanged(EntityUid uid, MassDriverConsoleComponent component, MassDriverThrowDistanceMessage args)
    {
        foreach (var massDriverUid in component.MassDrivers)
        {
            if (!TryComp<MassDriverComponent>(massDriverUid, out var massDriverComponent))
                continue;

            massDriverComponent.CurrentThrowDistance = Math.Clamp(
                args.Distance,
                massDriverComponent.MinThrowDistance,
                massDriverComponent.MaxThrowDistance);
            Dirty(massDriverUid, massDriverComponent);
        }

        SendState(uid, component);
    }

    private void OnUIOpen(EntityUid uid, MassDriverConsoleComponent component, BoundUIOpenedEvent args)
    {
        SendState(uid, component);
    }

    private void SendState(EntityUid consoleUid, MassDriverConsoleComponent component)
    {
        if (!_ui.HasUi(consoleUid, MassDriverConsoleUiKey.Key))
            return;

        var state = TryComp<MassDriverComponent>(component.MassDrivers.FirstOrNull(), out var massDriver)
            ? CreateState(massDriver)
            : new MassDriverComponentState();

        _ui.ServerSendUiMessage(consoleUid, MassDriverConsoleUiKey.Key, new MassDriverUpdateUIMessage(state));
    }

    private MassDriverComponentState CreateState(MassDriverComponent component)
    {
        return new MassDriverComponentState
        {
            MaxThrowSpeed = component.MaxThrowSpeed,
            MaxThrowDistance = component.MaxThrowDistance,
            MinThrowSpeed = component.MinThrowSpeed,
            MinThrowDistance = component.MinThrowDistance,
            CurrentThrowSpeed = component.CurrentThrowSpeed,
            CurrentThrowDistance = component.CurrentThrowDistance,
            CurrentMassDriverMode = component.Mode,
            Console = GetNetEntity(component.Console),
            Hacked = component.Hacked,
        };
    }
}
