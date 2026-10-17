using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// "Codewords" module.
/// Word categories are dynamically randomized per bomb.
/// </summary>
public sealed class CodewordsModule : BombModule
{
    public static readonly string[] BaseWords = new[]
    {
        "ALPHA", "BRAVO", "CHARLIE", "DELTA", "ECHO", "FOXTROT",
        "GOLF", "HOTEL", "INDIA", "JULIET", "KILO", "LIMA",
        "MIKE", "NOVEMBER", "OSCAR", "PAPA", "QUEBEC", "ROMEO",
        "SIERRA", "TANGO", "UNIFORM", "VICTOR", "WHISKEY", "XRAY",
        "YANKEE", "ZULU", "NINER", "ZERO", "CIPHER", "AGENT",
        "SECTOR", "VECTOR", "SIGNAL", "BEACON", "STATIC", "BOOM"
    };

    public Dictionary<char, string[]> ModuleWordColumns = new();
    public List<string> DisplayedWords = new();
    public int CorrectWordIndex;

    public CodewordsModule()
    {
        Type = BombModuleType.Codewords;
    }

    public static CodewordsModule Generate(IRobustRandom random)
    {
        var module = new CodewordsModule();

        // Generate randomized columns for this bomb
        var shuffledBase = BaseWords.ToList();
        random.Shuffle(shuffledBase);

        var keys = new[] { 'A', 'B', 'C', 'D', 'E', 'F' };
        for (int i = 0; i < keys.Length; i++)
        {
            module.ModuleWordColumns[keys[i]] = shuffledBase.Skip(i * 6).Take(6).ToArray();
        }

        // Pick a target column key
        var targetColumnKey = random.Pick(keys);
        var targetColumn = module.ModuleWordColumns[targetColumnKey];

        // Pick 2-3 words from the target column (these will be in the displayed list)
        var targetWords = targetColumn.ToList();
        random.Shuffle(targetWords);
        var fromTargetCount = random.Next(2, 4); // 2 or 3
        var selectedFromTarget = targetWords.Take(fromTargetCount).ToList();

        // The correct answer is the one that appears first in the column among displayed
        var correctWord = targetColumn.First(w => selectedFromTarget.Contains(w));

        // Pick remaining words from OTHER columns
        var otherWords = new List<string>();
        foreach (var kvp in module.ModuleWordColumns)
        {
            if (kvp.Key == targetColumnKey)
                continue;

            foreach (var word in kvp.Value)
            {
                if (!targetColumn.Contains(word) && !selectedFromTarget.Contains(word))
                    otherWords.Add(word);
            }
        }

        otherWords = otherWords.Distinct().ToList();
        random.Shuffle(otherWords);
        var fillCount = 6 - fromTargetCount;
        var filler = otherWords.Take(fillCount).ToList();

        var allDisplayed = new List<string>();
        allDisplayed.AddRange(selectedFromTarget);
        allDisplayed.AddRange(filler);
        random.Shuffle(allDisplayed);

        module.DisplayedWords = allDisplayed;
        module.CorrectWordIndex = allDisplayed.IndexOf(correctWord);

        return module;
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        return new CodewordsModuleState
        {
            IsSolved = IsSolved,
            Words = new List<string>(DisplayedWords),
            SelectedIndex = -1,
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        if (action is not SubmitCodewordAction submit)
            return false;

        if (submit.WordIndex < 0 || submit.WordIndex >= DisplayedWords.Count)
            return false;

        if (submit.WordIndex == CorrectWordIndex)
        {
            IsSolved = true;
            return true;
        }

        return false;
    }
}
