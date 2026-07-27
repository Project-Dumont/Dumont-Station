// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Hands;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Temperature;
using Content.Shared.Vampire.Components;

namespace Content.Server._Dumont.Religion.EntitySystems;

/// <summary>
/// Handles attempts to pick up protected unholy items.
/// </summary>
public sealed partial class UnholyItemSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;

    private static readonly TimeSpan HeavyStunTime = TimeSpan.FromSeconds(10);
    private const float FireStacks = 3f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnholyItemComponent, GettingPickedUpAttemptEvent>(
            OnPickupAttempt);
    }

    private void OnPickupAttempt(
        Entity<UnholyItemComponent> item,
        ref GettingPickedUpAttemptEvent args)
    {
        if (TryPunish(item, args.User))
            args.Cancel();
    }

    /// <summary>
    /// Checks whether punishment is enabled and whether the user is permitted
    /// to pick up the item.
    /// </summary>
    private bool TryPunish(
    Entity<UnholyItemComponent> item,
    EntityUid user)
    {
        if (!item.Comp.Punish)
            return false;

        if (HasComp<UnholyComponent>(user))
            return false;

        if (HasComp<BibleUserComponent>(user))
        {
            _popup.PopupEntity(
                Loc.GetString(
                    "unholy-item-bible-user-popup",
                    ("item", item.Owner)),
                item,
                user,
                PopupType.Medium);

            return false;
        }

        _popup.PopupEntity(
            Loc.GetString(
                "unholy-item-punishment-popup",
                ("item", item.Owner)),
            item,
            user,
            PopupType.LargeCaution);

        _stun.TryUpdateParalyzeDuration(
            user,
            HeavyStunTime);

        _flammable.AdjustFireStacks(user, FireStacks);
        _flammable.Ignite(user, item);

        // Tells the pickup handler to cancel acquisition of the item.
        return true;
    }
}
