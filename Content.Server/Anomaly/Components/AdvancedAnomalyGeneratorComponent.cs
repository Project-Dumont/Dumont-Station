// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Anomaly.Prototypes;
using Content.Shared.Materials;
using Content.Shared.Radio;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Anomaly.Components;

[RegisterComponent, AutoGenerateComponentPause, Access(typeof(AdvancedAnomalyGeneratorSystem))]
public sealed partial class AdvancedAnomalyGeneratorComponent : Component
{
    [DataField]
    public ProtoId<MaterialPrototype> RequiredMaterial = "Plasma";

    [DataField]
    public List<ProtoId<AdvancedAnomalyGenerationPrototype>> AllowedAnomalies = new();

    [DataField]
    public TimeSpan GenerationLength = TimeSpan.FromSeconds(8);

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromMinutes(5);

    // How far the generator can reach in a straight line, measured in tiles
    // Basically how far this thing can yeet stuff in a straight line
    [DataField]
    public int Range = 15;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan CooldownEnd;

    [DataField]
    public SoundSpecifier? GeneratingSound;

    [DataField]
    public SoundSpecifier? GeneratingFinishedSound;

    [DataField]
    public ProtoId<RadioChannelPrototype> AnnouncementChannel = "Science";
}
