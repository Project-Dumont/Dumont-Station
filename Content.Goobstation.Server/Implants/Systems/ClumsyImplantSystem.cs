// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 loltart <159829224+loltart@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Clumsy;
using Content.Shared.Implants;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class ClumsyImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Components.ClumsyImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<ClumsyComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }
    public void OnImplanted(Entity<ClumsyImplantComponent> clumsyImplant, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted is not { } implanted)
            return;

        EnsureComp<ClumsyComponent>(implanted);
    }

    public void OnUnimplanted(Entity<ClumsyComponent> ent, ref ImplantRemovedFromEvent args)
    {
        if (HasComp<ClumsyImplantComponent>(args.Implant))
            RemComp<ClumsyComponent>(ent);
    }
}
