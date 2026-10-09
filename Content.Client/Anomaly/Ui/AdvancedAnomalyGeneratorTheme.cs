// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;

namespace Content.Client.Anomaly.Ui;

// All the UI colors live here, so changing them updates the window cards and map marker together
// One color change here and the whole UI gets a new outfit
public static class AdvancedAnomalyGeneratorTheme
{
    public static readonly Color Accent = Color.FromHex("#b07cff");
    public static readonly Color Header = Color.FromHex("#d4b3ff");
    public static readonly Color MapWall = Color.FromHex("#8f63c9");
    public static readonly Color MapTile = Color.FromHex("#2a1a3d");

    public static readonly StyleBoxFlat Background = new() { BackgroundColor = Color.FromHex("#140d1c") };
    public static readonly StyleBoxFlat Panel = Box(Color.FromHex("#1f1430"), Color.FromHex("#5b3a86"));
    public static readonly StyleBoxFlat Card = Box(Color.FromHex("#1f1430"), Color.FromHex("#3f2a5c"));
    public static readonly StyleBoxFlat SelectedCard = Box(Color.FromHex("#3b2160"), Accent);

    private static StyleBoxFlat Box(Color background, Color border) => new()
    {
        BackgroundColor = background,
        BorderColor = border,
        BorderThickness = new Thickness(1),
    };
}
