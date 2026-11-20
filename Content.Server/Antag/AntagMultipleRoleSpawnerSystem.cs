// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag.Components;
using Robust.Shared.Random;

namespace Content.Server.Antag;

public sealed partial class AntagMultipleRoleSpawnerSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagMultipleRoleSpawnerComponent, AntagSelectEntityEvent>(OnSelectEntity);
    }

    private void OnSelectEntity(Entity<AntagMultipleRoleSpawnerComponent> ent, ref AntagSelectEntityEvent args)
    {
        if (args.Handled)
            return;

        foreach (var role in args.Definition.PrefRoles)
        {
            if (!ent.Comp.AntagRoleToPrototypes.TryGetValue(role, out var entProtos) || entProtos.Count == 0)
                continue;

            args.Entity = Spawn(ent.Comp.PickAndTake ? _random.PickAndTake(entProtos) : _random.Pick(entProtos));
            return;
        }
    }
}
