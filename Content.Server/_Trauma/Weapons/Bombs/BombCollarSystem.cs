// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Body.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Systems;
using Content.Trauma.Shared.Weapons.Bombs.Components;
using Robust.Shared.Containers;

namespace Content.Trauma.Server.Weapons.Bombs;

public sealed partial class BombCollarSystem : EntitySystem
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BombCollarComponent, TriggerEvent>(OnTrigger, before: [typeof(ExplodeOnTriggerSystem)]);
    }

    private void OnTrigger(Entity<BombCollarComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && args.Key != "timer" ||
            !_inventory.TryGetContainingSlot((ent.Owner, null, null), out var slot) || slot.Name != "neck" ||
            !_containers.TryGetContainingContainer(ent.Owner, out var container))
            return;

        var wearer = container.Owner;
        var severed = false;
        foreach (var (head, part) in _body.GetBodyChildrenOfType(wearer, BodyPartType.Head).ToArray())
        {
            if (!_body.TryDetachPart(head, part))
                continue;

            _transform.AttachToGridOrMap(head);
            severed = true;
        }

        if (!severed)
            return;

        _mobState.ChangeMobState(wearer, MobState.Dead, origin: ent.Owner);
        args.User = wearer;
        args.Handled = true;
    }
}
