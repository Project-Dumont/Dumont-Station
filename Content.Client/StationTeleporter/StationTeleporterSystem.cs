// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StationTeleporter;
using Robust.Client.GameObjects;
using StationTeleporterComponent = Content.Shared.StationTeleporter.Components.StationTeleporterComponent;

namespace Content.Client.StationTeleporter;

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
        if (ent.Comp.PortalLayerMap is null)
            return;

        if (!_appearance.TryGetData<Color>(ent, TeleporterPortalVisuals.Color, out var newColor))
            return;

        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(ent.Comp.PortalLayerMap, out var index))
            return;

        sprite.LayerSetColor(index, newColor);
    }
}
