// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mutatrix.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Mutatrix.Events;

[Serializable, NetSerializable]
public enum MutatrixUiKey : byte
{
    Key
}

/// <summary>
/// Sent by the client when the user selects a built-in/static transformation in the radial menu.
/// </summary>
[Serializable, NetSerializable]
public sealed class MutatrixSelectTransformationMessage : BoundUserInterfaceMessage
{
    public ProtoId<MutatrixTransformationPrototype> Transformation;

    public MutatrixSelectTransformationMessage()
    {
    }

    public MutatrixSelectTransformationMessage(ProtoId<MutatrixTransformationPrototype> transformation)
    {
        Transformation = transformation;
    }
}

/// <summary>
/// Sent by the client when the user selects a dynamically scanned entity prototype.
/// </summary>
[Serializable, NetSerializable]
public sealed class MutatrixSelectScannedPrototypeMessage : BoundUserInterfaceMessage
{
    public string EntityPrototype = string.Empty;

    public MutatrixSelectScannedPrototypeMessage()
    {
    }

    public MutatrixSelectScannedPrototypeMessage(string entityPrototype)
    {
        EntityPrototype = entityPrototype;
    }
}

/// <summary>
/// BUI state for the Mutatrix menu.
/// The client already knows static transformation metadata from prototypes.
/// Dynamic scan entries are sent as entity prototype IDs.
/// </summary>
[Serializable, NetSerializable]
public sealed class MutatrixBoundUserInterfaceState : BoundUserInterfaceState
{
    public HashSet<ProtoId<MutatrixTransformationPrototype>> Unlocked = new();
    public ProtoId<MutatrixTransformationPrototype>? Selected;

    public HashSet<string> DynamicUnlocked = new();
    public string? SelectedDynamic;

    public MutatrixBoundUserInterfaceState()
    {
    }

    public MutatrixBoundUserInterfaceState(
        HashSet<ProtoId<MutatrixTransformationPrototype>> unlocked,
        ProtoId<MutatrixTransformationPrototype>? selected,
        HashSet<string> dynamicUnlocked,
        string? selectedDynamic)
    {
        Unlocked = unlocked;
        Selected = selected;
        DynamicUnlocked = dynamicUnlocked;
        SelectedDynamic = selectedDynamic;
    }
}
