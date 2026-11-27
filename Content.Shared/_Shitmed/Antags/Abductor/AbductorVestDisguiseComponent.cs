// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Antags.Abductor;

[RegisterComponent]
public sealed partial class AbductorVestDisguiseComponent : Component;

[RegisterComponent]
public sealed partial class AbductorDisguiseStateComponent : Component
{
    [DataField]
    public EntityUid? RevertAction;

    [DataField]
    public string? OriginalName;

    [DataField]
    public DisguiseAppearance? OriginalAppearance;
}

public sealed partial class DisguiseRevertEvent : InstantActionEvent
{
    [DataField]
    public bool RaiseRenameEvents;
}

/// <summary>
/// Everything needed to put a humanoid back the way it was before being disguised.
/// </summary>
[DataDefinition]
public sealed partial class DisguiseAppearance
{
    [DataField]
    public ProtoId<SpeciesPrototype> Species;

    [DataField]
    public Color SkinColor;

    [DataField]
    public Color EyeColor;

    [DataField]
    public Sex Sex;

    [DataField]
    public Gender Gender;

    [DataField]
    public int Age;

    [DataField]
    public float Height;

    [DataField]
    public float Width;

    [DataField]
    public Dictionary<HumanoidVisualLayers, CustomBaseLayerInfo> CustomBaseLayers = new();

    [DataField]
    public MarkingSet Markings = new();
}
