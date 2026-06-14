using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// "Codewords" module.
/// 6 words are displayed. The defuser reads them out; the manual reader cross-references
/// them against word category columns to identify the single correct codeword.
/// </summary>
public sealed class CodewordsModule : BombModule
{
    /// <summary>
    /// Word categories for the manual. Each column has a header letter and a list of words.
    /// If 2 or more of the 6 displayed words appear in a column, the answer is the first
    /// word in that column that is also displayed.
    /// </summary>
    public static readonly Dictionary<char, string[]> WordColumns = new()
    {
        ['A'] = new[] { "ALPHA", "BRAVO", "CHARLIE", "DELTA", "ECHO", "FOXTROT" },
        ['B'] = new[] { "GOLF", "HOTEL", "INDIA", "JULIET", "KILO", "LIMA" },
        ['C'] = new[] { "MIKE", "NOVEMBER", "OSCAR", "PAPA", "QUEBEC", "ROMEO" },
        ['D'] = new[] { "SIERRA", "TANGO", "UNIFORM", "VICTOR", "WHISKEY", "XRAY" },
        ['E'] = new[] { "YANKEE", "ZULU", "NINER", "ZERO", "CIPHER", "AGENT" },
        ['F'] = new[] { "SECTOR", "VECTOR", "SIGNAL", "BEACON", "STATIC", "CIPHER" },
    };

    /// <summary>
    /// All words available across all columns (flattened, deduplicated).
    /// </summary>
    public static readonly string[] AllWords;

    static CodewordsModule()
    {
        var words = new HashSet<string>();
        foreach (var column in WordColumns.Values)
        {
            foreach (var word in column)
            {
                words.Add(word);
            }
        }
        AllWords = words.ToArray();
    }

    /// <summary>
    /// The 6 words displayed to the defuser.
    /// </summary>
    public List<string> DisplayedWords = new();

    /// <summary>
    /// The index of the correct word in DisplayedWords.
    /// </summary>
    public int CorrectWordIndex;

    public CodewordsModule()
    {
        Type = BombModuleType.Codewords;
    }

    public static CodewordsModule Generate(IRobustRandom random)
    {
        var module = new CodewordsModule();

        // Pick a target column
        var columns = WordColumns.Keys.ToList();
        var targetColumnKey = random.Pick(columns);
        var targetColumn = WordColumns[targetColumnKey];

        // Pick 2-3 words from the target column (these will be in the displayed list)
        var targetWords = targetColumn.ToList();
        random.Shuffle(targetWords);
        var fromTargetCount = random.Next(2, 4); // 2 or 3
        var selectedFromTarget = targetWords.Take(fromTargetCount).ToList();

        // The correct answer is the one that appears first in the column among displayed
        // Find which of the selected words comes first in the column
        var correctWord = targetColumn.First(w => selectedFromTarget.Contains(w));

        // Pick remaining words from OTHER columns (not the target)
        var otherWords = new List<string>();
        foreach (var kvp in WordColumns)
        {
            if (kvp.Key == targetColumnKey)
                continue;

            foreach (var word in kvp.Value)
            {
                // Make sure we don't accidentally pull in words also in the target column
                if (!targetColumn.Contains(word) && !selectedFromTarget.Contains(word))
                    otherWords.Add(word);
            }
        }

        // Deduplicate
        otherWords = otherWords.Distinct().ToList();
        random.Shuffle(otherWords);
        var fillCount = 6 - fromTargetCount;
        var filler = otherWords.Take(fillCount).ToList();

        // Combine and shuffle
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

        return false; // Wrong word — strike!
    }
}
