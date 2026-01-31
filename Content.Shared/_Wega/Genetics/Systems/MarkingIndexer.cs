using System.Linq;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Genetics.Systems
{
    public sealed class MarkingPrototypesIndexerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        private List<MarkingPrototypeInfo> _markingPrototypes = new List<MarkingPrototypeInfo>();
        private HashSet<string> _usedHexCombinations = new();
        private bool _isInitialized = false;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        }

        private void OnRoundRestart(RoundRestartCleanupEvent args)
        {
            _isInitialized = false;
            _markingPrototypes.Clear();
            _usedHexCombinations.Clear();
        }

        public List<MarkingPrototypeInfo> GetAllMarkingPrototypes()
        {
            if (!_isInitialized)
            {
                InitializeMarkingPrototypes();
            }

            return _markingPrototypes;
        }

        private void InitializeMarkingPrototypes()
        {
            _markingPrototypes.Clear();

            var allMarkingPrototypes = _prototypeManager.EnumeratePrototypes<MarkingPrototype>();
            foreach (var markingPrototype in allMarkingPrototypes)
            {
                if (markingPrototype.MarkingType is MarkingTypes.NonGenetics)
                    continue;

                var markingPrototypeInfo = new MarkingPrototypeInfo
                {
                    MarkingPrototypeId = markingPrototype.ID,
                    HexValue = GenerateHexValueForMarking(),
                    Species = GetPossibleSpeciesForMarking(markingPrototype)
                };

                _markingPrototypes.Add(markingPrototypeInfo);
            }

            _isInitialized = true;
        }

        private string[] GenerateHexValueForMarking()
        {
            string[] newHex;
            int attempts = 0;
            const int maxAttempts = 69;

            do
            {
                newHex = new[]
                {
                    _random.Next(0, 16).ToString("X1"),
                    _random.Next(0, 16).ToString("X1"),
                    _random.Next(0, 16).ToString("X1")
                };

                if (newHex[0] == "0" && newHex[1] == "0" && newHex[2] == "0")
                    continue;

                attempts++;

                if (attempts >= maxAttempts)
                {
                    return GetFallbackHexValue();
                }
            }
            while (_usedHexCombinations.Contains(string.Join("-", newHex)));

            _usedHexCombinations.Add(string.Join("-", newHex));
            return newHex;
        }

        private string[] GetFallbackHexValue()
        {
            for (int r = 1; r <= 15; r++)
                for (int g = 0; g <= 15; g++)
                    for (int b = 0; b <= 15; b++)
                    {
                        string candidate = $"{r:X1}-{g:X1}-{b:X1}";
                        if (!_usedHexCombinations.Contains(candidate))
                        {
                            _usedHexCombinations.Add(candidate);
                            return new[] { r.ToString("X1"), g.ToString("X1"), b.ToString("X1") };
                        }
                    }

            return new[] { "1", "1", "1" };
        }

        private string GetPossibleSpeciesForMarking(MarkingPrototype markingPrototype)
        {
            if (markingPrototype.SpeciesRestrictions != null && markingPrototype.SpeciesRestrictions.Count > 0)
            {
                return string.Join(", ", markingPrototype.SpeciesRestrictions.Select(species => species.ToString()));
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public sealed class MarkingPrototypeInfo
    {
        public string MarkingPrototypeId { get; set; } = string.Empty;
        public string[] HexValue { get; set; } = default!;
        public string Species { get; set; } = string.Empty;
    }
}
