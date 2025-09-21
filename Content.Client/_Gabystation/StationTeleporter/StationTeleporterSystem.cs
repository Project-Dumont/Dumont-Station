// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Gabystation.StationTeleporter;
using Robust.Client.GameObjects;
using StationTeleporterComponent = Content.Shared._Gabystation.StationTeleporter.Components.StationTeleporterComponent;

namespace Content.Client._Gabystation.StationTeleporter;

public sealed class StationTeleporterSystem : SharedStationTeleporterSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

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
            || !sprite.LayerMapTryGet(ent.Comp.PortalLayerMap, out var index))
            return;

        sprite.LayerSetColor(index, newColor);
    }
}
