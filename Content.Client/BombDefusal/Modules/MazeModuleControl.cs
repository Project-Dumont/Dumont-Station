using System;
using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

public sealed class MazeModuleControl : BaseModuleControl
{
    private readonly MazeGrid _mazeGrid;
    private readonly Button _btnUp;
    private readonly Button _btnDown;
    private readonly Button _btnLeft;
    private readonly Button _btnRight;

    public MazeModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-maze"));

        var mainLayout = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        ContentContainer.AddChild(mainLayout);

        // Left side: Maze grid
        _mazeGrid = new MazeGrid
        {
            MinSize = new Vector2(160, 160),
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        mainLayout.AddChild(_mazeGrid);

        // Right side: navigation buttons
        var dPadLayout = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Align = BoxContainer.AlignMode.Center,
            MinSize = new Vector2(100, 120),
            Margin = new Thickness(8, 0, 0, 0),
        };
        mainLayout.AddChild(dPadLayout);

        _btnUp = new Button
        {
            Text = "▲",
            MinSize = new Vector2(36, 36),
            HorizontalAlignment = HAlignment.Center,
        };
        _btnUp.OnPressed += _ => RaiseAction(new PressMazeDirectionAction(0, -1));
        dPadLayout.AddChild(_btnUp);

        var horizDPad = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 4),
        };
        dPadLayout.AddChild(horizDPad);

        _btnLeft = new Button
        {
            Text = "◄",
            MinSize = new Vector2(36, 36),
        };
        _btnLeft.OnPressed += _ => RaiseAction(new PressMazeDirectionAction(-1, 0));
        horizDPad.AddChild(_btnLeft);

        var spacer = new Control { MinSize = new Vector2(8, 0) };
        horizDPad.AddChild(spacer);

        _btnRight = new Button
        {
            Text = "►",
            MinSize = new Vector2(36, 36),
        };
        _btnRight.OnPressed += _ => RaiseAction(new PressMazeDirectionAction(1, 0));
        horizDPad.AddChild(_btnRight);

        _btnDown = new Button
        {
            Text = "▼",
            MinSize = new Vector2(36, 36),
            HorizontalAlignment = HAlignment.Center,
        };
        _btnDown.OnPressed += _ => RaiseAction(new PressMazeDirectionAction(0, 1));
        dPadLayout.AddChild(_btnDown);
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not MazeModuleState mazeState)
            return;

        _mazeGrid.WallFlags = mazeState.WallFlags;
        _mazeGrid.PlayerX = mazeState.PlayerX;
        _mazeGrid.PlayerY = mazeState.PlayerY;
        _mazeGrid.GoalX = mazeState.GoalX;
        _mazeGrid.GoalY = mazeState.GoalY;
        _mazeGrid.InvalidateArrange();

        var solved = mazeState.IsSolved;
        _btnUp.Disabled = solved;
        _btnDown.Disabled = solved;
        _btnLeft.Disabled = solved;
        _btnRight.Disabled = solved;
    }
}

public sealed class MazeGrid : Control
{
    public byte[] WallFlags = new byte[36];
    public int PlayerX;
    public int PlayerY;
    public int GoalX;
    public int GoalY;

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var size = Size;
        var side = Math.Min(size.X, size.Y);
        var cellSize = side / 6f;

        // Draw light grid lines
        var gridColor = Color.FromHex("#222233");
        for (int i = 0; i <= 6; i++)
        {
            handle.DrawLine(new Vector2(i * cellSize, 0), new Vector2(i * cellSize, side), gridColor);
            handle.DrawLine(new Vector2(0, i * cellSize), new Vector2(side, i * cellSize), gridColor);
        }

        // Draw walls
        var wallColor = Color.FromHex("#ff4444");
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 6; x++)
            {
                var flags = WallFlags[y * 6 + x];
                var cellLeft = x * cellSize;
                var cellRight = (x + 1) * cellSize;
                var cellTop = y * cellSize;
                var cellBottom = (y + 1) * cellSize;

                if ((flags & 1) != 0) // North
                    handle.DrawLine(new Vector2(cellLeft, cellTop), new Vector2(cellRight, cellTop), wallColor);
                if ((flags & 2) != 0) // South
                    handle.DrawLine(new Vector2(cellLeft, cellBottom), new Vector2(cellRight, cellBottom), wallColor);
                if ((flags & 4) != 0) // West
                    handle.DrawLine(new Vector2(cellLeft, cellTop), new Vector2(cellLeft, cellBottom), wallColor);
                if ((flags & 8) != 0) // East
                    handle.DrawLine(new Vector2(cellRight, cellTop), new Vector2(cellRight, cellBottom), wallColor);
            }
        }

        // Draw Goal (red triangle)
        var goalCenter = new Vector2((GoalX + 0.5f) * cellSize, (GoalY + 0.5f) * cellSize);
        var r = cellSize * 0.25f;
        var p1 = goalCenter + new Vector2(0, -r);
        var p2 = goalCenter + new Vector2(-r * 0.866f, r * 0.5f);
        var p3 = goalCenter + new Vector2(r * 0.866f, r * 0.5f);
        handle.DrawLine(p1, p2, Color.Red);
        handle.DrawLine(p2, p3, Color.Red);
        handle.DrawLine(p3, p1, Color.Red);

        // Draw Player (white circle)
        var playerCenter = new Vector2((PlayerX + 0.5f) * cellSize, (PlayerY + 0.5f) * cellSize);
        handle.DrawCircle(playerCenter, cellSize * 0.2f, Color.White);
    }
}
