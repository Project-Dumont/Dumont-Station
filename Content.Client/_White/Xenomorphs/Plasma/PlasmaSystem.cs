// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Alerts;
using Content.Shared._White.Xenomorphs.Plasma;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Robust.Client.GameObjects;

namespace Content.Client._White.Xenomorphs.Plasma;

public sealed class PlasmaSystem : SharedPlasmaSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlasmaVesselComponent, UpdateAlertSpriteEvent>(OnUpdateAlertSprite);
    }

    private void OnUpdateAlertSprite(EntityUid uid, PlasmaVesselComponent component, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != component.PlasmaAlert)
            return;

        var sprite = args.SpriteViewEnt;
        var plasma = Math.Clamp(component.Plasma.Int(), 0, 999);

        _sprite.LayerSetRsiState(sprite.Owner, PlasmaVisualLayers.Digit1, $"{plasma / 100 % 10}");
        _sprite.LayerSetRsiState(sprite.Owner, PlasmaVisualLayers.Digit2, $"{plasma / 10 % 10}");
        _sprite.LayerSetRsiState(sprite.Owner, PlasmaVisualLayers.Digit3, $"{plasma % 10}");
    }
}
