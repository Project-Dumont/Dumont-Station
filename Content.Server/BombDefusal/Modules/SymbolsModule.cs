using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// "Symbols/Keypads" module.
/// Symbol columns are dynamically randomized per bomb.
/// </summary>
public sealed class SymbolsModule : BombModule
{
    public static readonly int[][] SymbolColumns =
    {
        new[] { 0, 1, 2, 3, 4, 5, 6 },     // Column 1
        new[] { 7, 0, 6, 8, 9, 4, 10 },     // Column 2
        new[] { 11, 12, 8, 13, 14, 2, 9 },  // Column 3
        new[] { 15, 16, 13, 17, 18, 1, 12 },// Column 4
        new[] { 19, 18, 20, 15, 16, 21, 22},// Column 5
        new[] { 15, 7, 23, 24, 19, 11, 20 },// Column 6
    };

    public const int TotalSymbols = 25;

    public int[][] ModuleSymbolColumns = new int[6][];
    public List<int> DisplayedSymbols = new();
    public List<int> CorrectOrder = new();
    public List<int> PressedSymbols = new();

    public SymbolsModule()
    {
        Type = BombModuleType.Symbols;
    }

    public static SymbolsModule Generate(IRobustRandom random)
    {
        var module = new SymbolsModule();

        // Copy columns
        for (int i = 0; i < SymbolColumns.Length; i++)
        {
            module.ModuleSymbolColumns[i] = (int[])SymbolColumns[i].Clone();
        }

        // Randomly remap symbol IDs per bomb
        var symbolMapping = Enumerable.Range(0, TotalSymbols).ToList();
        random.Shuffle(symbolMapping);

        for (int i = 0; i < module.ModuleSymbolColumns.Length; i++)
        {
            for (int j = 0; j < module.ModuleSymbolColumns[i].Length; j++)
            {
                var oldId = module.ModuleSymbolColumns[i][j];
                module.ModuleSymbolColumns[i][j] = symbolMapping[oldId];
            }
        }

        // Pick a random column
        var columnIndex = random.Next(module.ModuleSymbolColumns.Length);
        var column = module.ModuleSymbolColumns[columnIndex];

        // Pick 4 distinct symbols from this column
        var indices = Enumerable.Range(0, column.Length).ToList();
        random.Shuffle(indices);
        var chosenColumnPositions = indices.Take(4).ToList();

        // The displayed symbols (in random order on the buttons)
        var displaySymbols = chosenColumnPositions.Select(pos => column[pos]).ToList();
        random.Shuffle(displaySymbols);
        module.DisplayedSymbols = displaySymbols;

        // The correct order is the order they appear in the column (top to bottom)
        chosenColumnPositions.Sort();
        var correctSymbolOrder = chosenColumnPositions.Select(pos => column[pos]).ToList();

        // Convert to indices into DisplayedSymbols
        module.CorrectOrder = correctSymbolOrder.Select(sym => module.DisplayedSymbols.IndexOf(sym)).ToList();

        return module;
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        return new SymbolsModuleState
        {
            IsSolved = IsSolved,
            SymbolIds = new List<int>(DisplayedSymbols),
            PressedSymbols = new List<int>(PressedSymbols),
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        if (action is not PressSymbolAction pressSymbol)
            return false;

        if (pressSymbol.SymbolIndex < 0 || pressSymbol.SymbolIndex >= DisplayedSymbols.Count)
            return false;

        if (PressedSymbols.Contains(pressSymbol.SymbolIndex))
            return true; // no-op

        var expectedIndex = CorrectOrder[PressedSymbols.Count];

        if (pressSymbol.SymbolIndex == expectedIndex)
        {
            PressedSymbols.Add(pressSymbol.SymbolIndex);
            if (PressedSymbols.Count == CorrectOrder.Count)
                IsSolved = true;
            return true;
        }

        PressedSymbols.Clear();
        return false;
    }
}
