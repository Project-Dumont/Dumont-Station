// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Gangs;
using Content.Server.Forensics;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.Gangs;
public sealed class GangGraffitiSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GangGraffitiComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, GangGraffitiComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<CleansForensicsComponent>(args.Used))
            return;

        QueueDel(uid);
        args.Handled = true;
    }
}
