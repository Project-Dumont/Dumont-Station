// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Clothing;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems.PathSpecific.Cosmos;

public sealed partial class MovementIgnoreGravityClothingSystem : EntitySystem
{
    [Dependency] private SharedGravitySystem _gravity = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.SubscribeWithRelay<MovementIgnoreGravityClothingComponent, IsWeightlessEvent>(OnIsWeightless,
            held: false,
            baseEvent: false);

        SubscribeLocalEvent<MovementIgnoreGravityClothingComponent, ClothingGotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<MovementIgnoreGravityClothingComponent, ClothingGotUnequippedEvent>(OnUnequip);
    }

    private void OnUnequip(Entity<MovementIgnoreGravityClothingComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        _gravity.RefreshWeightless(args.Wearer);
    }

    private void OnEquip(Entity<MovementIgnoreGravityClothingComponent> ent, ref ClothingGotEquippedEvent args)
    {
        _gravity.RefreshWeightless(args.Wearer);
    }

    private void OnIsWeightless(Entity<MovementIgnoreGravityClothingComponent> ent, ref IsWeightlessEvent args)
    {
        args.IsWeightless = ent.Comp.Weightless;
        args.Handled = true;
    }
}
