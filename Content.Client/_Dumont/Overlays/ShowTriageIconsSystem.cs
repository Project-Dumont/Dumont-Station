// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Overlays;
using Content.Shared._Dumont.Overlays;
using Content.Shared._Dumont.Triage;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Dumont.Overlays;

/// <summary>
/// Desenha a moldura de triagem por cima do ícone de saúde, só para quem usa MedHUD.
/// </summary>
public sealed class ShowTriageIconsSystem : EquipmentHudSystem<ShowTriageIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriageTagComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(Entity<TriageTagComponent> ent, ref GetStatusIconsEvent args)
    {
        if (!IsActive)
            return;

        if (_prototype.TryIndex(ent.Comp.Level, out var icon) && !icon.DrawOnHealthBar)
            args.StatusIcons.Add(icon);
    }
}
