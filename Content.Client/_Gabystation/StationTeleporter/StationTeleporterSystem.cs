// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <41803390+Kyoth25f@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Gabystation.StationTeleporter;
using Content.Shared._Gabystation.StationTeleporter.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Gabystation.StationTeleporter;

public sealed class StationTeleporterSystem : SharedStationTeleporterSystem
{
    [Dependency]
    private readonly SharedAppearanceSystem _appearance = default!;

    [Dependency]
    private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationTeleporterComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(Entity<StationTeleporterComponent> ent, ref AppearanceChangeEvent args)
    {
        if (ent.Comp.PortalLayerMap is null
            || !_appearance.TryGetData<Color>(ent, TeleporterPortalVisuals.Color, out var newColor)
            || !TryComp<SpriteComponent>(ent, out var sprite)
            || !_spriteSystem.LayerMapTryGet((ent, sprite), ent.Comp.PortalLayerMap, out var index, false))
            return;

        _spriteSystem.LayerSetColor((ent, sprite), index, newColor);
    }
}
