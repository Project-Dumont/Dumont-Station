// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Pinpointer.UI;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;

namespace Content.Client.Anomaly.Ui;

public sealed class AdvancedAnomalyGeneratorNavMapControl : NavMapControl
{
    private readonly SharedMapSystem _map;

    public event Action<Vector2i>? TileSelected;

    public AdvancedAnomalyGeneratorNavMapControl()
    {
        _map = EntManager.System<SharedMapSystem>();

        WallColor = AdvancedAnomalyGeneratorTheme.MapWall;
        TileColor = AdvancedAnomalyGeneratorTheme.MapTile;
        BackgroundColor = Color.FromSrgb(TileColor.WithAlpha(BackgroundOpacity));
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function != EngineKeyFunctions.UIClick
            || (StartDragPosition - args.PointerLocation.Position).Length() > MinDragDistance
            || MapUid is not { } gridUid
            || !EntManager.TryGetComponent<MapGridComponent>(gridUid, out var grid)
            || !EntManager.TryGetComponent<PhysicsComponent>(gridUid, out var physics))
            return;

        // Turns the screen click into a spot on the grid basically undoing how NavMapControl draws the map
        // Basically translates "I clicked here" into "okay but where is here on the grid?"
        var unscaled = (args.PointerLocation.Position - GlobalPixelPosition - MidPointVector) / MinimapScale;
        var local = new Vector2(unscaled.X, -unscaled.Y) + Offset + physics.LocalCenter;

        TileSelected?.Invoke(_map.LocalToTile(gridUid, grid, new EntityCoordinates(gridUid, local)));
    }
}
