// SPDX-FileCopyrightText: 2025 MarkerWicker (OmuStation)
// SPDX-License-Identifier: AGPL-3.0-or-later
// Ported from OmuStation PRs #150 and #1613; adapted for Dumont.

using Content.Goobstation.Common.Speech;
using Content.Shared._Omu.Clothing.Components;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Shared._Omu.Traits;

/// <summary>
///     info.
/// </summary>
public sealed class CyberneticBeastSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyberneticBeastComponent, GetSpeechSoundEvent>(OnSpeechSoundNeeded);
    }

    /// <summary>
    ///     Changes this cybernetic beast's speech sound while wearing a mantle. The specific sound that gets played can be specified in the CyberneticBeastComponent.
    /// </summary>
    private void OnSpeechSoundNeeded(Entity<CyberneticBeastComponent> ent, ref GetSpeechSoundEvent args)
    {
        if (!args.Handled && IsWearingMantle(ent))
        {
            args.SpeechSoundProtoId = ent.Comp.SpeechSoundWhileWearingMantle;
            args.Handled = true;
        }
    }

    /// <summary>
    ///     Whether the specified cybernetic beast is wearing a mantle.
    /// </summary>
    private bool IsWearingMantle(Entity<CyberneticBeastComponent> ent)
    {
        // if the passed entity has an inventory component, and in that inventory one item has a cybernetic mantle component, the player must be wearing a cybernetic mantle.
        if (EntityManager.TryGetComponent<InventoryComponent>(ent, out var inventoryComponent))
            if (_inventorySystem.TryGetInventoryEntity<CyberneticMantleComponent>(new Entity<InventoryComponent?>(ent, inventoryComponent), out var cyberneticMantle))
                return true;

        return false;
    }

}
