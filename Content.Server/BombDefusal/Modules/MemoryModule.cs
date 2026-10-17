using System.Collections.Generic;
using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

public enum MemoryRuleType : byte
{
    Position,
    Label,
    SamePositionAsStage,
    SameLabelAsStage
}

public sealed class MemoryStageRule
{
    public MemoryRuleType[] Types = new MemoryRuleType[4]; // Keyed by (DisplayNumber - 1)
    public int[] Values = new int[4];
}

public sealed class MemoryModule : BombModule
{
    public const int MaxStages = 5;
    public int CurrentStageIndex; // 0 to 4

    // Rules generated for all 5 stages
    public List<MemoryStageRule> StageRules = new();

    // Actual state generated per stage
    public List<int> Displays = new();
    public List<List<int>> StageButtonLabels = new();

    // Player inputs recorded per stage (index and label)
    public List<int> PressedPositions = new();
    public List<int> PressedLabels = new();

    public MemoryModule()
    {
        Type = BombModuleType.Memory;
    }

    public static MemoryModule Generate(IRobustRandom random)
    {
        var module = new MemoryModule();

        // 1. Generate rules for all 5 stages
        for (int stage = 0; stage < MaxStages; stage++)
        {
            var rule = new MemoryStageRule();
            for (int displayIdx = 0; displayIdx < 4; displayIdx++)
            {
                // Select rule type. If stage 0, only Position or Label.
                var allowedTypes = new List<MemoryRuleType> { MemoryRuleType.Position, MemoryRuleType.Label };
                if (stage > 0)
                {
                    allowedTypes.Add(MemoryRuleType.SamePositionAsStage);
                    allowedTypes.Add(MemoryRuleType.SameLabelAsStage);
                }

                var ruleType = random.Pick(allowedTypes);
                rule.Types[displayIdx] = ruleType;

                switch (ruleType)
                {
                    case MemoryRuleType.Position:
                        rule.Values[displayIdx] = random.Next(0, 4); // 0-indexed position
                        break;
                    case MemoryRuleType.Label:
                        rule.Values[displayIdx] = random.Next(1, 5); // Label 1-4
                        break;
                    case MemoryRuleType.SamePositionAsStage:
                        rule.Values[displayIdx] = random.Next(0, stage); // Stage index
                        break;
                    case MemoryRuleType.SameLabelAsStage:
                        rule.Values[displayIdx] = random.Next(0, stage); // Stage index
                        break;
                }
            }
            module.StageRules.Add(rule);
        }

        // 2. Pre-generate display numbers and button configurations for all 5 stages
        for (int stage = 0; stage < MaxStages; stage++)
        {
            module.Displays.Add(random.Next(1, 5)); // Display 1-4
            var labels = new List<int> { 1, 2, 3, 4 };
            random.Shuffle(labels);
            module.StageButtonLabels.Add(labels);
        }

        return module;
    }

    public int GetCorrectButtonIndexForStage(int stageIdx)
    {
        if (stageIdx < 0 || stageIdx >= MaxStages)
            return 0;

        var display = Displays[stageIdx];
        var displayIdx = display - 1;

        var rule = StageRules[stageIdx];
        var type = rule.Types[displayIdx];
        var val = rule.Values[displayIdx];

        var labels = StageButtonLabels[stageIdx];

        switch (type)
        {
            case MemoryRuleType.Position:
                return Math.Clamp(val, 0, 3);
            case MemoryRuleType.Label:
                return Math.Max(0, labels.IndexOf(val));
            case MemoryRuleType.SamePositionAsStage:
                var prevStagePosIdx = Math.Clamp(val, 0, stageIdx - 1);
                var prevPos = PressedPositions[prevStagePosIdx];
                return prevPos;
            case MemoryRuleType.SameLabelAsStage:
                var prevStageLabelIdx = Math.Clamp(val, 0, stageIdx - 1);
                var prevLabel = PressedLabels[prevStageLabelIdx];
                return Math.Max(0, labels.IndexOf(prevLabel));
        }

        return 0;
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        return new MemoryModuleState
        {
            IsSolved = IsSolved,
            CurrentStage = CurrentStageIndex,
            DisplayNumber = CurrentStageIndex < MaxStages ? Displays[CurrentStageIndex] : 1,
            ButtonLabels = CurrentStageIndex < MaxStages ? StageButtonLabels[CurrentStageIndex] : new List<int> { 1, 2, 3, 4 },
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        if (action is not PressMemoryButtonAction pressMem)
            return false;

        var pressedIdx = pressMem.ButtonIndex;
        if (pressedIdx < 0 || pressedIdx >= 4)
            return false;

        var correctIdx = GetCorrectButtonIndexForStage(CurrentStageIndex);

        if (pressedIdx == correctIdx)
        {
            // Record player input
            var label = StageButtonLabels[CurrentStageIndex][pressedIdx];
            PressedPositions.Add(pressedIdx);
            PressedLabels.Add(label);

            CurrentStageIndex++;

            if (CurrentStageIndex >= MaxStages)
            {
                IsSolved = true;
            }

            return true;
        }

        // Wrong button — strike! Reset memory game progress.
        CurrentStageIndex = 0;
        PressedPositions.Clear();
        PressedLabels.Clear();
        return false;
    }
}
