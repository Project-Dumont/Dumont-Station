// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.MassDriver.Components;

/// <summary>
/// Stores configuration and state data for a mass driver.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MassDriverComponent : Component
{
    /// <summary>
    /// Current throw speed.
    /// </summary>
    [DataField]
    public float CurrentThrowSpeed = 10.0f;

    /// <summary>
    /// Max throw speed configurable from the console.
    /// </summary>
    [DataField]
    public float MaxThrowSpeed = 10.0f;

    /// <summary>
    /// Min throw speed configurable from the console.
    /// </summary>
    [DataField]
    public float MinThrowSpeed = 5.0f;

    /// <summary>
    /// Current throw distance.
    /// </summary>
    [DataField]
    public float CurrentThrowDistance = 5.0f;

    /// <summary>
    /// Max throw distance configurable from the console.
    /// </summary>
    [DataField]
    public float MaxThrowDistance = 15.0f;

    /// <summary>
    /// Min throw distance configurable from the console.
    /// </summary>
    [DataField]
    public float MinThrowDistance = 2.0f;

    /// <summary>
    /// Speed and distance lost for each additional launched entity.
    /// </summary>
    [DataField]
    public float ThrowCountDelta = 0.5f;

    /// <summary>
    /// Delay before launching intersecting entities.
    /// </summary>
    [DataField]
    public TimeSpan ThrowDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Time the launch animation and sound should stay active after a throw.
    /// </summary>
    [DataField]
    public TimeSpan LaunchAnimationTime = TimeSpan.FromSeconds(1.1);

    /// <summary>
    /// Current driver mode.
    /// </summary>
    [DataField]
    public MassDriverMode Mode = MassDriverMode.Auto;

    /// <summary>
    /// Power load while launching.
    /// </summary>
    [DataField]
    public float LaunchPowerLoad = 1000f;

    /// <summary>
    /// Idle power load.
    /// </summary>
    [DataField]
    public float MassDriverPowerLoad = 100f;

    /// <summary>
    /// Port used to receive launch signals in manual mode.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> LaunchPort = "Launch";

    /// <summary>
    /// Whether the security wire is cut or pulsed.
    /// </summary>
    public bool Hacked;

    /// <summary>
    /// Throw speed while hacked.
    /// </summary>
    [DataField]
    public float HackedSpeedRewrite = 20f;

    /// <summary>
    /// Linked control console.
    /// </summary>
    [DataField]
    public EntityUid? Console;
}
