using System.Collections.Generic;
using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

public sealed class MorseCodeModule : BombModule
{
    public static readonly Dictionary<string, float> WordFrequencies = new()
    {
        { "SHELL", 3.505f },
        { "HALLS", 3.515f },
        { "SLICK", 3.522f },
        { "TRICK", 3.532f },
        { "BOXES", 3.535f },
        { "LEAKS", 3.542f },
        { "STROB", 3.545f },
        { "BOMBS", 3.552f },
        { "FLICK", 3.555f },
        { "MEDIC", 3.565f },
        { "CLOAK", 3.572f },
        { "SPACE", 3.575f },
        { "CLEAN", 3.582f },
        { "SHARK", 3.592f },
        { "STEAM", 3.595f },
        { "GHOST", 3.600f }
    };

    private static readonly Dictionary<char, string> MorseAlphabet = new()
    {
        { 'A', ".-" },    { 'B', "-..." },  { 'C', "-.-." },  { 'D', "-.." },
        { 'E', "." },     { 'F', "..-." },  { 'G', "--." },   { 'H', "...." },
        { 'I', ".." },    { 'J', ".---" },  { 'K', "-.-" },   { 'L', ".-.." },
        { 'M', "--" },    { 'N', "-." },    { 'O', "---" },   { 'P', ".--." },
        { 'Q', "--.-" },  { 'R', ".-." },   { 'S', "..." },   { 'T', "-" },
        { 'U', "..-" },   { 'V', "...-" },  { 'W', ".--" },   { 'X', "-..-" },
        { 'Y', "-.--" },  { 'Z', "--.." }
    };

    public string TargetWord = string.Empty;
    public float CorrectFrequency;
    public string MorseSequence = string.Empty;

    public List<float> Frequencies = new();
    public int CurrentFrequencyIndex;

    public MorseCodeModule()
    {
        Type = BombModuleType.MorseCode;
    }

    public static MorseCodeModule Generate(IRobustRandom random)
    {
        var module = new MorseCodeModule();

        // Populate and sort the frequency list
        module.Frequencies = WordFrequencies.Values.OrderBy(f => f).ToList();

        // Pick target word
        var words = WordFrequencies.Keys.ToList();
        module.TargetWord = random.Pick(words);
        module.CorrectFrequency = WordFrequencies[module.TargetWord];

        // Generate Morse sequence string
        var morseLetters = module.TargetWord.Select(c => MorseAlphabet[c]);
        module.MorseSequence = string.Join(" ", morseLetters);

        // Pick random starting frequency index
        module.CurrentFrequencyIndex = random.Next(0, module.Frequencies.Count);

        return module;
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        return new MorseCodeModuleState
        {
            IsSolved = IsSolved,
            MorseSequence = MorseSequence,
            CurrentFrequency = Frequencies[CurrentFrequencyIndex]
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        switch (action)
        {
            case CycleMorseFrequencyAction cycle:
                if (cycle.Up)
                {
                    CurrentFrequencyIndex = (CurrentFrequencyIndex + 1) % Frequencies.Count;
                }
                else
                {
                    CurrentFrequencyIndex = (CurrentFrequencyIndex - 1 + Frequencies.Count) % Frequencies.Count;
                }
                return true; // Cycling frequency doesn't give a strike

            case SubmitMorseAction:
                var freq = Frequencies[CurrentFrequencyIndex];
                if (MathHelper.CloseTo(freq, CorrectFrequency, 0.001f))
                {
                    IsSolved = true;
                    return true;
                }

                return false; // Wrong frequency — strike!

            default:
                return false;
        }
    }
}
