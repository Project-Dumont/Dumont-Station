// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Examine;
using Content.Shared._Trauma.Nuclear;
using Robust.Shared.Map;

namespace Content.Client._Trauma.Nuclear;

public sealed partial class ClientNuclearMachineSystem : NuclearMachineSystem
{
    [SubscribeLocalEvent]
    private void OnExamined(Entity<NuclearMachineComponent> ent, ref ClientExaminedEvent args)
    {
        SpawnAttachedTo(ent.Comp.ArrowPrototype, new EntityCoordinates(ent, 0, 0)); // Dumont
    }
}
