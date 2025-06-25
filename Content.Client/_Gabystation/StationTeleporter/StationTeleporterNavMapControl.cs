// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Client.Pinpointer.UI;
using Robust.Client.Graphics;

namespace Content.Client._Gabystation.StationTeleporter;

public sealed partial class StationTeleporterNavMapControl : NavMapControl
{
    public HashSet<(Vector2, Vector2)> LinkedTeleportersCoordinates = new();

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

        foreach (var link in LinkedTeleportersCoordinates)
        {
            var pos1 = Vector2.Transform(link.Item1, _transformSystem.GetInvWorldMatrix(_xform)) - GetOffset();
            pos1 = ScalePosition(new Vector2(pos1.X, -pos1.Y));

            var pos2 = Vector2.Transform(link.Item2, _transformSystem.GetInvWorldMatrix(_xform)) - GetOffset();
            pos2 = ScalePosition(new Vector2(pos2.X, -pos2.Y));

            handle.DrawLine(pos1, pos2, _connectedLineColor);
        }
    }
}
