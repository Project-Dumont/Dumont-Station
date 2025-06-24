// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.StationTeleporter.Components;

/// <summary>
/// Stores a reference to a specific teleporter. Can be inserted into the teleporter control console so that the console can control this teleporter
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedStationTeleporterSystem))]
public sealed partial class TeleporterChipComponent : Component
{
    /// <summary>
    /// Uid of the teleporter this chip is synced with.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedTeleporter;

    /// <summary>
    /// The already localized name of the teleporter the chip has synced is copied into this field. This information is used when examining the chip.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ConnectedName = string.Empty;

    [DataField]
    public SoundSpecifier RecordSound = new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f),
    };
}
