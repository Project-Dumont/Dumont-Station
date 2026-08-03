// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.SubFloor;
using Content.Shared._Dumont.Overlays;
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
        SubscribeLocalEvent<ScannerVisionComponent, ItemToggledEvent>(OnVisionToggled);
        SubscribeLocalEvent<MesonVisionComponent, ItemToggledEvent>(OnMesonToggled);
    }

    private void OnMesonToggled(Entity<MesonVisionComponent> ent, ref ItemToggledEvent args)
    {
        if (ent.Comp.Enabled == args.Activated)
            return;

        ent.Comp.Enabled = args.Activated;
        Dirty(ent);
    }

    /// <summary>
    /// o toggle liga só o que o óculos declarou saber fazer no prototype
    /// </summary>
    private void OnVisionToggled(Entity<ScannerVisionComponent> ent, ref ItemToggledEvent args)
    {
        var structure = ent.Comp.ShowsStructure && args.Activated;

        if (ent.Comp.Structure == structure)
            return;

        ent.Comp.Structure = structure;
        Dirty(ent);
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
