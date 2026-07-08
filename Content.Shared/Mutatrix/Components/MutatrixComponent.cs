// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mutatrix.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Mutatrix.Components;

/// <summary>
/// Main component for the Mutatrix device.
///
/// This belongs on the wearable item, not on the user. Runtime state that should
/// survive body swaps belongs in <see cref="MutatrixDnaComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMutatrixSystem))]
public sealed partial class MutatrixComponent : Component
{
    /// <summary>
    /// Action granted to the wearer while this device is equipped.
    /// Keep this as string because GetItemActionsEvent currently exposes a string overload.
    /// </summary>
    [DataField]
    public string Action = "ActionMutatrixOpenMenu";

    /// <summary>
    /// Runtime menu action entity currently attached to the wearer.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// Action granted to manually capture DNA from nearby mobs.
    /// </summary>
    [DataField]
    public string CaptureAction = "ActionMutatrixCaptureDNA";

    /// <summary>
    /// Runtime capture action entity currently attached to the wearer.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? CaptureActionEntity;

    /// <summary>
    /// Maximum scan range. 2.5 tiles approximates a 5x5 tile area centered on the wearer.
    /// </summary>
    [DataField]
    public float ScanRange = 2.5f;

    /// <summary>
    /// Default scan time. Individual transformations can override this.
    /// </summary>
    [DataField]
    public float ScanTime = 10f;

    /// <summary>
    /// Cooldown after reverting from a Mutatrix transformation, in seconds.
    /// 90 seconds = 1 minute and 30 seconds.
    /// </summary>
    [DataField]
    public float RevertCooldown = 90f;

    /// <summary>
    /// Whether the device should automatically scan nearby unknown species.
    /// Disabled by default because the Mutatrix now exposes a manual Capture DNA action.
    /// </summary>
    [DataField]
    public bool AutoScan;

    /// <summary>
    /// How often the wearer looks for a new unknown mob while not already scanning.
    /// This avoids searching every frame.
    /// </summary>
    [DataField]
    public float AutoScanInterval = 1f;
}
