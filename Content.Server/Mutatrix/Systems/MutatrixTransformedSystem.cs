// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Mutatrix.Components;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server.Mutatrix.Systems;

/// <summary>
/// Safety monitor for Mutatrix polymorph bodies.
/// Handles crit/death and ghost/disconnect/body detach for every Mutatrix body,
/// including mobs that skip Critical and go straight to Dead.
/// Also preserves items picked up while transformed before reverting.
/// </summary>
public sealed class MutatrixTransformedSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MutatrixTransformedComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MutatrixTransformedComponent, PolymorphedEntityComponent>();
        while (query.MoveNext(out var uid, out _, out var polymorphed))
        {
            if (!IsUncontrolled(uid))
                continue;

            TryRevert(uid, polymorphed);
        }
    }

    private void OnMobStateChanged(Entity<MutatrixTransformedComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Critical && args.NewMobState != MobState.Dead)
            return;

        if (!TryComp<PolymorphedEntityComponent>(ent.Owner, out var polymorphed))
            return;

        TryRevert(ent.Owner, polymorphed);
    }

    private void TryRevert(EntityUid uid, PolymorphedEntityComponent polymorphed)
    {
        if (Deleted(uid) || Deleted(polymorphed.Parent))
            return;

        PreserveHeldItemsBeforeRevert(uid, polymorphed.Parent);

        _polymorph.Revert((uid, polymorphed));
    }

    /// <summary>
    /// Saves items picked up while transformed.
    /// Non-humanoid Mutatrix forms use Inventory=None, so normal Polymorph revert
    /// does not move hand items back. Without this, picked items can be deleted
    /// together with the temporary body.
    ///
    /// Behavior:
    /// - Try to move each held item to the same hand ID on the original body.
    /// - If that hand does not exist or is occupied, try any empty hand.
    /// - If no hand is available, leave the item dropped on the floor.
    /// </summary>
    private void PreserveHeldItemsBeforeRevert(EntityUid transformed, EntityUid parent)
    {
        if (!TryComp<HandsComponent>(transformed, out var transformedHands))
            return;

        var heldItems = new List<(EntityUid Item, string HandId)>();

        foreach (var handId in _hands.EnumerateHands((transformed, transformedHands)))
        {
            if (!_hands.TryGetHeldItem((transformed, transformedHands), handId, out var held) || held == null)
                continue;

            heldItems.Add((held.Value, handId));
        }

        if (heldItems.Count == 0)
            return;

        TryComp<HandsComponent>(parent, out var parentHands);

        foreach (var (item, handId) in heldItems)
        {
            if (Deleted(item))
                continue;

            // Remove from the temporary body's hand first. If we cannot remove it,
            // do not try to insert it into the parent.
            if (!_hands.TryDrop((transformed, transformedHands), item, checkActionBlocker: false, doDropInteraction: false))
                continue;

            if (parentHands == null)
                continue;

            if (_hands.TryGetHand((parent, parentHands), handId, out _)
                && _hands.HandIsEmpty((parent, parentHands), handId)
                && _hands.TryPickup(parent, item, handId, checkActionBlocker: false))
            {
                continue;
            }

            // Fallback: if the matching hand is missing/occupied, use any free hand.
            // If this also fails, the item remains dropped on the floor.
            _hands.TryPickupAnyHand(parent, item, checkActionBlocker: false, handsComp: parentHands);
        }
    }

    private bool IsUncontrolled(EntityUid uid)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return true;

        if (actor.PlayerSession.Status != SessionStatus.InGame)
            return true;

        return actor.PlayerSession.AttachedEntity is not { } attached || attached != uid;
    }
}
