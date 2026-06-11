// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Materials;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Anomaly.Components;

[RegisterComponent]
public sealed partial class AdvancedAnomalyGeneratorComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<MaterialPrototype> RequiredMaterial = "Plasma";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaterialCost = 1500;

    [DataField]
    public List<ProtoId<Content.Shared.Anomaly.Prototypes.AdvancedAnomalyGenerationPrototype>> AllowedAnomalies = new();

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public string? LastMessage;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan GenerationLength = TimeSpan.FromSeconds(8);

    [DataField]
    public SoundSpecifier? GeneratingSound;

    [DataField]
    public SoundSpecifier? GeneratingFinishedSound;

    [DataField]
    public string AnnouncementChannel = "Science";
}
