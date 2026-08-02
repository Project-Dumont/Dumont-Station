// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.SubFloor;
using Content.Shared._Dumont.SubFloor;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.SubFloor;

namespace Content.Server._Dumont.SubFloor;

/// <summary>
/// liga o scanner de subsolo pelo ItemToggle, que é como roupa vestida é
/// acionada (magboots, capacete de hardsuit). o scanner de mão do upstream só
/// liga por ActivateInWorld, o que exige o item na mão e não serve pra óculos
/// </summary>
public sealed class TrayScannerToggleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WornTrayScannerComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<WornTrayScannerComponent, ActivateInWorldEvent>(OnActivate,
            before: [typeof(TrayScannerSystem)]);
    }

    private void OnToggled(Entity<WornTrayScannerComponent> ent, ref ItemToggledEvent args)
    {
        if (!TryComp<TrayScannerComponent>(ent, out var scanner) || scanner.Enabled == args.Activated)
            return;

        scanner.Enabled = args.Activated;
        Dirty(ent.Owner, scanner);
    }

    /// <summary>
    /// scanner que é roupa não liga na mão. sem isso o clique do upstream
    /// acende o óculos segurado e ele revela o subsolo fora do rosto
    /// </summary>
    private void OnActivate(Entity<WornTrayScannerComponent> ent, ref ActivateInWorldEvent args)
    {
        args.Handled = true;
    }
}
