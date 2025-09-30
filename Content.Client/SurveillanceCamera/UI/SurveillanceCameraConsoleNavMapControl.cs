// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Hagvan <22118902+Hagvan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 John Willis <143434770+CerberusWolfie@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Goobstation Start
// I really want to put this in the goobstation namespace
// but it breaks references in the xaml and as a result the the xaml.cs
using Content.Client.Pinpointer.UI;

namespace Content.Client.SurveillanceCamera.UI
{
    public sealed partial class SurveillanceCameraConsoleNavMapControl : NavMapControl
    {

        public SurveillanceCameraConsoleNavMapControl() : base()
        {
            // Set colors
            TileColor = new Color(30, 57, 67);
            WallColor = new Color(192, 192, 192);
            BackgroundColor = Color.FromSrgb(TileColor.WithAlpha(BackgroundOpacity));
        }

        protected override void UpdateNavMap()
        {
            base.UpdateNavMap();
        }
    }
}
// Goobstation End
