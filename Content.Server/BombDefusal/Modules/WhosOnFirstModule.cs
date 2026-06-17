using System.Collections.Generic;
using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

public sealed class WhosOnFirstModule : BombModule
{
    public static readonly string[] WordPool = new[]
    {
        "READY", "FIRST", "NO", "BLANK", "NOTHING", "YES", "WHAT",
        "UHHH", "LEFT", "RIGHT", "MIDDLE", "OKAY", "WAIT", "PRESS", "YOU"
    };

    public const int MaxStages = 3;
    public int CurrentStageIndex; // 0 to 2

    // Rules generated per bomb
    public Dictionary<string, int> DisplayToPositionMap = new();
    public Dictionary<string, List<string>> WordPriorityLists = new();

    // Stage inputs
    public List<string> Displays = new();
    public List<List<string>> StageButtonLabels = new(); // 6 labels per stage

    public WhosOnFirstModule()
    {
        Type = BombModuleType.WhosOnFirst;
    }

    public static WhosOnFirstModule Generate(IRobustRandom random)
    {
        var module = new WhosOnFirstModule();

        // 1. Generate randomized display word mappings (Step 1)
        foreach (var word in WordPool)
        {
            module.DisplayToPositionMap[word] = random.Next(0, 6);
        }

        // 2. Generate randomized priority lists (Step 2)
        foreach (var word in WordPool)
        {
            var priority = WordPool.ToList();
            random.Shuffle(priority);
            module.WordPriorityLists[word] = priority;
        }

        // 3. Pre-generate stages
        for (int i = 0; i < MaxStages; i++)
        {
            // Pick random display word
            module.Displays.Add(random.Pick(WordPool));

            // Pick 6 distinct button labels
            var labels = WordPool.ToList();
            random.Shuffle(labels);
            module.StageButtonLabels.Add(labels.Take(6).ToList());
        }

        return module;
    }

    public int GetCorrectButtonIndexForStage(int stageIdx)
    {
        if (stageIdx < 0 || stageIdx >= MaxStages)
            return 0;

        var display = Displays[stageIdx];
        var labels = StageButtonLabels[stageIdx];

        // Step 1: Find position index
        var pos = DisplayToPositionMap[display];
        pos = Math.Clamp(pos, 0, 5);

        // Step 2: Look at label at position
        var word = labels[pos];

        // Step 3: Check priority list for that word
        var priority = WordPriorityLists[word];

        // Find the first word in priority list that is present in the labels list
        foreach (var pWord in priority)
        {
            var idx = labels.IndexOf(pWord);
            if (idx >= 0)
                return idx;
        }

        return 0;
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        return new WhosOnFirstModuleState
        {
            IsSolved = IsSolved,
            DisplayWord = CurrentStageIndex < MaxStages ? Displays[CurrentStageIndex] : "READY",
            ButtonLabels = CurrentStageIndex < MaxStages ? StageButtonLabels[CurrentStageIndex] : new List<string> { "YES", "NO", "WAIT", "OKAY", "PRESS", "FIRST" },
            CurrentStage = CurrentStageIndex
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        if (action is not PressWhosOnFirstButtonAction pressWof)
            return false;

        var pressedIdx = pressWof.ButtonIndex;
        if (pressedIdx < 0 || pressedIdx >= 6)
            return false;

        var correctIdx = GetCorrectButtonIndexForStage(CurrentStageIndex);

        if (pressedIdx == correctIdx)
        {
            CurrentStageIndex++;
            if (CurrentStageIndex >= MaxStages)
            {
                IsSolved = true;
            }
            return true;
        }

        // Wrong button — strike! Reset progress.
        CurrentStageIndex = 0;
        return false;
    }
}
