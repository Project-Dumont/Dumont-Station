using System.Collections.Generic;
using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

public sealed class MazeModule : BombModule
{
    public byte[,] Walls = new byte[6, 6]; // N=1, S=2, W=4, E=8
    public int PlayerX;
    public int PlayerY;
    public int GoalX;
    public int GoalY;

    public int CurrentX;
    public int CurrentY;

    public List<string> PathDirections = new();

    public MazeModule()
    {
        Type = BombModuleType.Maze;
    }

    public static MazeModule Generate(IRobustRandom random, string serialNumber)
    {
        var module = new MazeModule();

        // 1. Generate Maze using DFS
        var visited = new bool[6, 6];
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                module.Walls[x, y] = 1 | 2 | 4 | 8; // All closed
            }
        }

        void Visit(int cx, int cy)
        {
            visited[cx, cy] = true;
            var directions = new List<(int dx, int dy, byte wall, byte opposite)>
            {
                (0, -1, 1, 2), // North
                (0, 1, 2, 1),  // South
                (-1, 0, 4, 8), // West
                (1, 0, 8, 4)   // East
            };
            random.Shuffle(directions);
            foreach (var dir in directions)
            {
                var nx = cx + dir.dx;
                var ny = cy + dir.dy;
                if (nx >= 0 && nx < 6 && ny >= 0 && ny < 6 && !visited[nx, ny])
                {
                    module.Walls[cx, cy] &= (byte)~dir.wall;
                    module.Walls[nx, ny] &= (byte)~dir.opposite;
                    Visit(nx, ny);
                }
            }
        }
        Visit(0, 0);

        // 2. Select Start and Goal
        module.PlayerX = random.Next(0, 6);
        module.PlayerY = random.Next(0, 6);
        module.CurrentX = module.PlayerX;
        module.CurrentY = module.PlayerY;

        do
        {
            module.GoalX = random.Next(0, 6);
            module.GoalY = random.Next(0, 6);
        } while (module.GoalX == module.PlayerX && module.GoalY == module.PlayerY);

        // 3. Find correct path using BFS
        module.PathDirections = FindPath(module);

        return module;
    }

    public static List<string> FindPath(MazeModule module)
    {
        var start = (module.PlayerX, module.PlayerY);
        var goal = (module.GoalX, module.GoalY);

        var queue = new Queue<(int x, int y, List<string> path)>();
        queue.Enqueue((start.Item1, start.Item2, new List<string>()));

        var visited = new HashSet<(int, int)>();
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (cx, cy, path) = queue.Dequeue();
            if (cx == goal.Item1 && cy == goal.Item2)
            {
                return path;
            }

            var cellWalls = module.Walls[cx, cy];

            // Try North
            if ((cellWalls & 1) == 0 && cy - 1 >= 0 && !visited.Contains((cx, cy - 1)))
            {
                visited.Add((cx, cy - 1));
                var nPath = new List<string>(path) { "UP" };
                queue.Enqueue((cx, cy - 1, nPath));
            }
            // Try South
            if ((cellWalls & 2) == 0 && cy + 1 < 6 && !visited.Contains((cx, cy + 1)))
            {
                visited.Add((cx, cy + 1));
                var nPath = new List<string>(path) { "DOWN" };
                queue.Enqueue((cx, cy + 1, nPath));
            }
            // Try West
            if ((cellWalls & 4) == 0 && cx - 1 >= 0 && !visited.Contains((cx - 1, cy)))
            {
                visited.Add((cx - 1, cy));
                var nPath = new List<string>(path) { "LEFT" };
                queue.Enqueue((cx - 1, cy, nPath));
            }
            // Try East
            if ((cellWalls & 8) == 0 && cx + 1 < 6 && !visited.Contains((cx + 1, cy)))
            {
                visited.Add((cx + 1, cy));
                var nPath = new List<string>(path) { "RIGHT" };
                queue.Enqueue((cx + 1, cy, nPath));
            }
        }

        return new List<string>();
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        var wallFlagsArray = new byte[36];
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 6; x++)
            {
                wallFlagsArray[y * 6 + x] = Walls[x, y];
            }
        }

        return new MazeModuleState
        {
            IsSolved = IsSolved,
            WallFlags = wallFlagsArray,
            PlayerX = CurrentX,
            PlayerY = CurrentY,
            GoalX = GoalX,
            GoalY = GoalY,
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        if (action is not PressMazeDirectionAction mazeAction)
            return false;

        var dx = mazeAction.Dx;
        var dy = mazeAction.Dy;

        // Determine target cell
        var tx = CurrentX + dx;
        var ty = CurrentY + dy;

        if (tx < 0 || tx >= 6 || ty < 0 || ty >= 6)
            return false; // Out of bounds is a strike

        // Check if there is a wall in this direction from current cell
        var cellWalls = Walls[CurrentX, CurrentY];
        bool hasWall = false;

        if (dx == 0 && dy == -1) // North
            hasWall = (cellWalls & 1) != 0;
        else if (dx == 0 && dy == 1) // South
            hasWall = (cellWalls & 2) != 0;
        else if (dx == -1 && dy == 0) // West
            hasWall = (cellWalls & 4) != 0;
        else if (dx == 1 && dy == 0) // East
            hasWall = (cellWalls & 8) != 0;
        else
            return false;

        if (hasWall)
        {
            return false; // Wrong move — strike!
        }

        CurrentX = tx;
        CurrentY = ty;

        if (CurrentX == GoalX && CurrentY == GoalY)
        {
            IsSolved = true;
        }

        return true;
    }
}
