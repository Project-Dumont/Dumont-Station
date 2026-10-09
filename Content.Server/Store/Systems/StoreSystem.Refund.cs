// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

using Content.Server.Store.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Store.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
// Dumont end

namespace Content.Server.Store.Systems;

public sealed partial class StoreSystem
{
    [Dependency] private IGameTiming _timing = default!;

    private void InitializeRefund()
    {
        SubscribeLocalEvent<StoreComponent, EntityTerminatingEvent>(OnStoreTerminating);
        SubscribeLocalEvent<StoreRefundComponent, EntityTerminatingEvent>(OnRefundTerminating);
        SubscribeLocalEvent<StoreRefundComponent, EntRemovedFromContainerMessage>(OnEntityRemoved);
        SubscribeLocalEvent<StoreRefundComponent, EntInsertedIntoContainerMessage>(OnEntityInserted);
        SubscribeLocalEvent<StoreRefundComponent, ActionPerformedEvent>(OnActionPerformed);
        SubscribeLocalEvent<StoreRefundComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<StoreRefundComponent, AttemptShootEvent>(OnShootAttempt);
        // TODO: Handle guardian refund disabling when guardians support refunds.
    }

    private void OnEntityRemoved(Entity<StoreRefundComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        CheckDisableRefund(ent);
    }

    private void OnEntityInserted(Entity<StoreRefundComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        CheckDisableRefund(ent);
    }

    private void OnActionPerformed(Entity<StoreRefundComponent> ent, ref ActionPerformedEvent args)
    {
        CheckDisableRefund(ent);
    }

    private void OnUseInHand(Entity<StoreRefundComponent> ent, ref UseInHandEvent args)
    {
        CheckDisableRefund(ent);
    }

    private void OnShootAttempt(Entity<StoreRefundComponent> ent, ref AttemptShootEvent args)
    {
        if (args.Cancelled)
            return;

        CheckDisableRefund(ent);
    }

    private void OnStoreTerminating(Entity<StoreComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.BoughtEntities.Count <= 0)
            return;

        foreach (var boughtEnt in ent.Comp.BoughtEntities)
        {
            if (!TryComp<StoreRefundComponent>(boughtEnt, out var refundComp))
                continue;

            refundComp.StoreEntity = null;
        }
    }

    private void OnRefundTerminating(Entity<StoreRefundComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.StoreEntity == null)
            return;

        var ev = new RefundEntityDeletedEvent(ent);
        RaiseLocalEvent(ent.Comp.StoreEntity.Value, ref ev);
    }

    private void CheckDisableRefund(Entity<StoreRefundComponent> ent)
    {
        var component = ent.Comp;

        if (component.StoreEntity == null || !TryComp<StoreComponent>(component.StoreEntity.Value, out var storeComp) || !storeComp.RefundAllowed)
            return;

        var endTime = component.BoughtTime + component.DisableTime;

        if (IsOnStartingMap(component.StoreEntity.Value, storeComp) && _timing.CurTime < endTime)
            return;

        // <Trauma>
        DisableListingRefund(component.Data);
        //DisableRefund(component.StoreEntity.Value, storeComp);
        // </Trauma>
    }
}
