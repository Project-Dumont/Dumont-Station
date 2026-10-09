// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;

namespace Content.Shared.Store;

// Dumont start
public abstract partial class SharedStoreSystem
{
    public bool TryAddCurrency<T>(Dictionary<T, Content.Goobstation.Maths.FixedPoint.FixedPoint2> currency, EntityUid uid, Content.Shared.Store.Components.StoreComponent? store = null) where T : notnull
    {
        return TryAddCurrency(currency.ToDictionary(entry => (Robust.Shared.Prototypes.ProtoId<CurrencyPrototype>) entry.Key.ToString()!,  entry => entry.Value), uid, store);
    }

    private void InitializeImplantStores()
    {
        SubscribeLocalEvent<ImplantedComponent, GetStoreEvent>(OnImplantedGetStore);
        SubscribeLocalEvent<ImplantedComponent, CurrencyInsertAttemptEvent>(OnImplantedCurrencyInsert);
    }

    private void OnImplantedGetStore(Entity<ImplantedComponent> ent, ref GetStoreEvent args)
    {
        if (args.Handled || ent.Comp.ImplantContainer == null)
            return;

        foreach (var implant in ent.Comp.ImplantContainer.ContainedEntities)
        {
            RaiseLocalEvent(implant, ref args);
            if (args.Handled)
                return;
        }
    }

    private void OnImplantedCurrencyInsert(Entity<ImplantedComponent> ent, ref CurrencyInsertAttemptEvent args)
    {
        if (args.Cancelled || ent.Comp.ImplantContainer == null)
            return;

        var relay = new ImplantRelayEvent<CurrencyInsertAttemptEvent>(args, ent.Owner);
        foreach (var implant in ent.Comp.ImplantContainer.ContainedEntities)
        {
            RaiseLocalEvent(implant, relay);
            if (args.Cancelled)
                return;
        }
    }
}
// Dumont end
