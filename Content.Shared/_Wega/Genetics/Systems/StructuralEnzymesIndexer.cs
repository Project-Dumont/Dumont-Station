using System.Linq;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Genetics.Systems
{
    public sealed class StructuralEnzymesIndexerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        private List<EnzymesPrototypeInfo> _enzymesPrototypes = new List<EnzymesPrototypeInfo>();
        private bool _isInitialized = false;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        }

        private void OnRoundRestart(RoundRestartCleanupEvent args)
        {
            _isInitialized = false;
            _enzymesPrototypes.Clear();
        }

        public List<EnzymesPrototypeInfo> GetAllEnzymesPrototypes()
        {
            if (!_isInitialized)
            {
                InitializeEnzymesPrototypes();
            }

            return _enzymesPrototypes;
        }

        private void InitializeEnzymesPrototypes()
        {
            _enzymesPrototypes.Clear();

            var allEnzymesPrototypes = _prototypeManager.EnumeratePrototypes<StructuralEnzymesPrototype>().ToList();
            _random.Shuffle(allEnzymesPrototypes);

            int maxBlocks = 54;
            int blocksToAdd = Math.Min(allEnzymesPrototypes.Count, maxBlocks);
            for (var i = 0; i < blocksToAdd; i++)
            {
                var enzymesPrototypeInfo = new EnzymesPrototypeInfo
                {
                    EnzymesPrototypeId = allEnzymesPrototypes[i].ID,
                    Order = i + 1
                };

                _enzymesPrototypes.Add(enzymesPrototypeInfo);
            }

            for (var i = blocksToAdd; i < maxBlocks; i++)
            {
                var emptyBlock = new EnzymesPrototypeInfo
                {
                    Order = i + 1
                };

                _enzymesPrototypes.Add(emptyBlock);
            }

            _random.Shuffle(_enzymesPrototypes);

            // Last block
            var lastEmptyBlock = new EnzymesPrototypeInfo
            {
                Order = 55
            };

            _enzymesPrototypes.Add(lastEmptyBlock);

            _isInitialized = true;
        }
    }
}
