// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mutatrix.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Mutatrix.Components;

/// <summary>
/// Runtime scan state. The server owns the scan; the client can use this for HUD.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMutatrixSystem))]
public sealed partial class MutatrixScanComponent : Component
{
    /// <summary>
    /// Entity currently being scanned.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Target;

    /// <summary>
    /// Entity prototype ID that will be unlocked if the scan completes.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? ScannedPrototype;

    /// <summary>
    /// Server time at which the scan started.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan StartTime;

    /// <summary>
    /// Server time at which the scan should finish.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

    /// <summary>
    /// Current progress from 0 to 1. This is intentionally approximate and HUD-only.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Progress;
}
