// SPDX-FileCopyrightText: 2024 Lgibb18 <65973111+Lgibb18@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared._Sunrise.Disease;
namespace Content.Client._Sunrise.Disease;

public sealed class DiseaseRoleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<ClientInfectEvent>(OnInfect);
    }



    private void OnInfect(ClientInfectEvent ev)
    {

        var target = GetEntity(ev.Infected);
        var performer = GetEntity(ev.Owner);

        if (!TryComp<HumanoidAppearanceComponent>(target, out var body))
            return;

        var sick = EnsureComp<SickComponent>(target);
        sick.owner = performer;
        sick.Inited = true;
        if (TryComp<DiseaseRoleComponent>(performer, out var comp))
        {
            comp.Infected.Add(target);
        }
    }

}
