// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Pinpointer.UI;
using Robust.Client.UserInterface;
using Robust.Shared.Map;
using Robust.Shared.Input;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;

namespace Content.Client.Anomaly.Ui;

public sealed partial class AdvancedAnomalyGeneratorNavMapControl : NavMapControl
{
    private SharedMapSystem _map = default!;

    public event Action<Vector2i>? TileSelected;

    public AdvancedAnomalyGeneratorNavMapControl()
    {
        _map = EntManager.System<SharedMapSystem>();
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (MapUid is not { } gridUid ||
            !EntManager.TryGetComponent<MapGridComponent>(gridUid, out var grid) ||
            !EntManager.TryGetComponent<PhysicsComponent>(gridUid, out var physics))
        {
            return;
        }

        if ((StartDragPosition - args.PointerLocation.Position).Length() > MinDragDistance)
            return;

        var offset = Offset + physics.LocalCenter;
        var localPosition = args.PointerLocation.Position - GlobalPixelPosition;
        var unscaledPosition = (localPosition - MidPointVector) / MinimapScale;
        var gridLocalPosition = new Vector2(unscaledPosition.X, -unscaledPosition.Y) + offset;
        var selectedTile = _map.LocalToTile(gridUid, grid, new EntityCoordinates(gridUid, gridLocalPosition));

        TileSelected?.Invoke(selectedTile);
    }
}
