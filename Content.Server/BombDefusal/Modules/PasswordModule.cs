using System.Collections.Generic;
using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

public sealed class PasswordModule : BombModule
{
    public static readonly string[] PredefinedWords = new[]
    {
        "ABOUT", "AFTER", "AGAIN", "GREAT", "HOUSE",
        "LARGE", "NEVER", "OTHER", "PLACE", "PLANT",
        "POINT", "RIGHT", "SMALL", "SOUND", "STUDY",
        "THEIR", "THERE", "THESE", "THING", "THINK",
        "THREE", "WATER", "WHERE", "WHICH", "WORLD", "WRITE"
    };

    public List<string> PoolWords = new();
    public string TargetWord = string.Empty;
    public List<List<char>> Columns = new();
    public List<int> SelectedIndices = new() { 0, 0, 0, 0, 0 };

    public PasswordModule()
    {
        Type = BombModuleType.Password;
    }

    public static PasswordModule Generate(IRobustRandom random)
    {
        var module = new PasswordModule();

        // 1. Pick 8-12 words randomly to form the current pool of possible passwords shown in the rules
        var shuffledPool = PredefinedWords.ToList();
        random.Shuffle(shuffledPool);
        module.PoolWords = shuffledPool.Take(random.Next(8, 13)).ToList();

        // 2. Target word is picked from the pool
        module.TargetWord = random.Pick(module.PoolWords);

        // 3. Generate 6 letters for each of the 5 columns
        for (int col = 0; col < 5; col++)
        {
            var targetChar = module.TargetWord[col];
            var colLetters = new HashSet<char> { targetChar };

            // Fill up to 6 distinct letters
            while (colLetters.Count < 6)
            {
                var randChar = (char)('A' + random.Next(0, 26));
                colLetters.Add(randChar);
            }

            var colList = colLetters.ToList();
            random.Shuffle(colList);
            module.Columns.Add(colList);
            
            // Set random starting selection index
            module.SelectedIndices[col] = random.Next(0, 6);
        }

        return module;
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        return new PasswordModuleState
        {
            IsSolved = IsSolved,
            Columns = Columns.Select(c => c.ToList()).ToList(),
            SelectedIndices = new List<int>(SelectedIndices)
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        switch (action)
        {
            case CyclePasswordColumnAction cycle:
                var col = cycle.ColumnIndex;
                if (col < 0 || col >= 5)
                    return false;

                if (cycle.Up)
                {
                    SelectedIndices[col] = (SelectedIndices[col] + 1) % 6;
                }
                else
                {
                    SelectedIndices[col] = (SelectedIndices[col] - 1 + 6) % 6;
                }
                return true; // Cycling is never wrong

            case SubmitPasswordAction:
                // Construct the word spelled by the columns
                var word = "";
                for (int i = 0; i < 5; i++)
                {
                    word += Columns[i][SelectedIndices[i]];
                }

                if (word == TargetWord)
                {
                    IsSolved = true;
                    return true;
                }

                return false; // Wrong word — strike!

            default:
                return false;
        }
    }
}
