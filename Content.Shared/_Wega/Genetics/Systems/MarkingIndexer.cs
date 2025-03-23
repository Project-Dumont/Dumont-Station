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
            return new[] { $"{_random.Next(0, 16):X1}", $"{_random.Next(0, 16):X1}", $"{_random.Next(0, 16):X1}" };
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
