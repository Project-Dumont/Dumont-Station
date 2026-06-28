using System.Linq;
using Content.Shared.Store;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Emag.Components;

namespace Content.Server._Starlight.Store.Conditions;

/// <summary>
/// Permite que itens da loja sejam filtrados com base no ID do comprador.
/// Lê o AccessReader da máquina.
/// </summary>
public sealed partial class BuyerAccessCondition : ListingCondition
{
    [DataField("access", required: false)]
    public string? access = null;

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;
        var buyer = args.Buyer;
        var store = args.StoreEntity;

        // Se a máquina não tiver AccessReader (não tiver tranca), libera a compra pra todos.
        if (store == null || !ent.TryGetComponent<AccessReaderComponent>(store, out var accessReader))
            return true;

        var _accessReader = ent.System<AccessReaderSystem>();

        if (access != null)
        {
            var accesses = _accessReader.FindAccessTags(buyer);
            if (accesses.Any(a => a.ToString() == access))
                return true;
        }
        // libera tudo se a máquina tiver sido emaggada
        else if (_accessReader.IsAllowed(buyer, store.Value, accessReader)
                || ent.HasComponent<EmaggedComponent>(store.Value))
        {
            return true;
        }

        return false; // Acesso negado!
    }
}