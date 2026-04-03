// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared._Gabystation.Interaction.Events;
using Content.Shared._Gabystation.Interaction.Components;
using Content.Shared.Popups;

namespace Content.Shared._Gabystation.Interaction.EntitySystems;

public sealed class RecallItemSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RecallBoundItemComponent, RecallBoundItemEvent>(OnRecall);
        SubscribeLocalEvent<BoundRecallComponent, EntityTerminatingEvent>(OnBoundItemDeleted);
        SubscribeLocalEvent<RecallBoundItemComponent, EntityTerminatingEvent>(OnUserDeleted);
    }

    private void OnRecall(Entity<RecallBoundItemComponent> ent, ref RecallBoundItemEvent args)
    {
        var user = args.Performer;

        if (ent.Comp.BoundItem is null)
            return;

        var item = ent.Comp.BoundItem.Value;

        if (!Exists(item))
        {
            ent.Comp.BoundItem = null;
            Dirty(ent);
            return;
        }

        if (_hands.IsHolding(user, item))
        {
            _popup.PopupEntity(Loc.GetString("recall-item-already-held"), user, user);
            args.Handled = true;
            return;
        }

        if (_hands.TryPickupAnyHand(user, item))
            _popup.PopupEntity(Loc.GetString("recall-item-success"), user, user);
        else
            _popup.PopupEntity(Loc.GetString("recall-item-hands-full"), user, user);

        args.Handled = true;
    }

    private void OnBoundItemDeleted(Entity<BoundRecallComponent> ent, ref EntityTerminatingEvent args)
    {
        var item = ent.Owner;

        var query = EntityQueryEnumerator<RecallBoundItemComponent>();

        while (query.MoveNext(out var userUid, out var recallComp))
        {
            if (recallComp.BoundItem != item)
                continue;

            recallComp.BoundItem = null;

            if (recallComp.RecallActionEntity is { } actionEnt && Exists(actionEnt))
                QueueDel(actionEnt);

            recallComp.RecallActionEntity = null;

            Dirty(userUid, recallComp);
        }
    }

    private void OnUserDeleted(Entity<RecallBoundItemComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.RecallActionEntity is { } action
            && Exists(action))
            QueueDel(action);

        if (ent.Comp.BoundItem is { } boundEnt
            && TryComp<BoundRecallComponent>(boundEnt, out var boundComp))
        {
            boundComp.BoundUser = null;
            Dirty(boundEnt, boundComp);
        }
    }
}
