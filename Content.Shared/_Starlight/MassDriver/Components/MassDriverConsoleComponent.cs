// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.MassDriver.Components;

/// <summary>
/// Stores linked mass drivers for a mass driver control console.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MassDriverConsoleComponent : Component
{
    /// <summary>
    /// Linked mass drivers.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> MassDrivers = new();

    /// <summary>
    /// Source port used to link the console to mass drivers.
    /// </summary>
    [DataField]
    public ProtoId<SourcePortPrototype> LinkingPort = "MassDriverConsoleSender";
}
