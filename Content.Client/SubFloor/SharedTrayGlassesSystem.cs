using Content.Shared.Inventory;
using Content.Shared.SubFloor;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Clothing;

public sealed class TrayGlassesSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemToggleComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<ItemToggleComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<ItemToggleComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnToggled(Entity<ItemToggleComponent> ent, ref ItemToggledEvent args)
    {
        // Verifica se o óculos está equipado no slot de olhos
        if (_container.TryGetContainingContainer((ent.Owner, null, null), out var container) &&
            _inventory.TryGetSlotEntity(container.Owner, "eyes", out var worn) &&
            worn == ent.Owner)
        {
            UpdateTrayEffects(ent.Owner, args.Activated);
        }
    }

    private void OnGotEquipped(Entity<ItemToggleComponent> ent, ref ClothingGotEquippedEvent args)
    {
        // Se equipou o óculos e ele já estava ligado, ativa a visão
        if (_toggle.IsActivated(ent.Owner))
        {
            UpdateTrayEffects(ent.Owner, true);
        }
    }

    private void OnGotUnequipped(Entity<ItemToggleComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        // Se tirou o óculos do rosto, desliga a visão imediatamente
        UpdateTrayEffects(ent.Owner, false);
    }

    private void UpdateTrayEffects(EntityUid ent, bool state)
    {
        if (state)
        {
            // Adiciona o componente de varredura no óculos
            EnsureComp<TrayScannerComponent>(ent);
        }
        else
        {
            // Remove o componente de varredura do óculos
            RemComp<TrayScannerComponent>(ent);
        }
    }
}
