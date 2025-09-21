// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Gabystation.StationTeleporter.Components;

/// <summary>
/// Console that allows you to manage the StationTeleporterComponent.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedStationTeleporterSystem))]
public sealed partial class StationTeleporterConsoleComponent : Component
{
    /// <summary>
    /// When initialized, teleporters can automatically generate chips in this console if they have matching AutoLinkKey.
    /// </summary>
    [DataField]
    public string? AutoLinkKey = null;

    /// <summary>
    /// EntProto of chip that will be spawn inside teleport if <see cref="AutoLinkKey" /> will match. It will be automatically filled with data of linked teleport.
    /// </summary>
    [DataField]
    public EntProtoId? AutoLinkChipsProto = "TeleporterChipBlank";

    /// <summary>
    /// Teleport that is currently selected in console UI. When selected - changes behaviour on next teleport selection.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SelectedTeleporter;

    /// <summary>
    /// Portals created by this console will be colored in the specified color. This can be used to make Syndicate portals blood red.
    /// </summary>
    [DataField]
    public Color PortalColor = Color.White;

    /// <summary>
    /// A storage from which all coordinate chips are scanned.
    /// </summary>
    [DataField]
    public string ChipStorageName = "storagebase";

    [DataField, AutoPausedField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(1);

}
