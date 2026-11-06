// SPDX-FileCopyrightText: 2025 MarkerWicker (OmuStation)
// SPDX-License-Identifier: AGPL-3.0-or-later
// Ported from OmuStation PRs #150 and #1613; adapted for Dumont.

using Content.Shared.Actions;

namespace Content.Shared._Omu.Clothing;

/// <summary>
///     Raised on an entity before a piece of ToggleableClothing is going to place that entity into its container.
/// </summary>
/// <param name="ClothingParent">The parent of the entity to be put into the container. Also the parent of the entity with ToggleableClothing.</param>
[ByRefEvent]
public readonly record struct AboutToEnterToggleableClothingContainerEvent(EntityUid ClothingParent);

public sealed partial class CyberneticMantleLightToggleEvent : InstantActionEvent
{
}
