// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Client.Pinpointer.UI;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Physics;

namespace Content.Client._Gabystation.StationTeleporter;

public sealed partial class StationTeleporterNavMapControl : NavMapControl
{
    // Testar passar o MapCoordinates aqui
    public HashSet<(EntityUid, EntityUid)> LinkedTeleporters = new();

    private readonly Color _connectedLineColor = Color.Aqua;
    private readonly Color _navmapWallColor = new Color(32, 96, 128);
    private readonly Color _navmapTileColor = new Color(12, 50, 69);

    private readonly SharedTransformSystem _transformSystem;

    public StationTeleporterNavMapControl()
    {
        _transformSystem = EntManager.System<SharedTransformSystem>();

        MaxSelectableDistance = 30f;

        WallColor = _navmapWallColor;
        TileColor = _navmapTileColor;
        BackgroundColor = Color.FromSrgb(TileColor.WithAlpha(BackgroundOpacity));

        PostWallDrawingAction += DrawAllTeleporterLinks;
    }

    private void DrawAllTeleporterLinks(DrawingHandleScreen handle)
    {
        if (_xform is null)
            return;

        foreach (var link in LinkedTeleporters)
        {
            if (!EntManager.TryGetComponent<TransformComponent>(link.Item1, out var xform1)
                || xform1.MapID != _xform.MapID
                || !EntManager.TryGetComponent<TransformComponent>(link.Item2, out var xform2)
                || xform2.MapID != _xform.MapID)
                continue;

            var mapPos1 = _transformSystem.GetMapCoordinates(link.Item1, xform1);
            var pos1 = Vector2.Transform(mapPos1.Position, _transformSystem.GetInvWorldMatrix(_xform)) - GetOffset();
            pos1 = ScalePosition(new Vector2(pos1.X, -pos1.Y));

            var mapPos2 = _transformSystem.GetMapCoordinates(link.Item2, xform2);
            var pos2 = Vector2.Transform(mapPos2.Position, _transformSystem.GetInvWorldMatrix(_xform)) - GetOffset();
            pos2 = ScalePosition(new Vector2(pos2.X, -pos2.Y));

            handle.DrawLine(pos1, pos2, _connectedLineColor);
        }
    }
}
