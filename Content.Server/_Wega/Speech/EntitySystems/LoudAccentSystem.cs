using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class LoudAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<LoudAccentComponent, AccentGetEvent>(OnAccent);
        }

        private static readonly IReadOnlyList<string> Exclamations = new List<string>
        {
            "!!!", "!!", "!?", "?!"
        }.AsReadOnly();

        public string Accentuate(string message)
        {
            var loudMessage = message.ToUpperInvariant();
            if (_random.Prob(0.8f))
            {
                loudMessage += _random.Pick(Exclamations);
            }

            return loudMessage;
        }

        private void OnAccent(EntityUid uid, LoudAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
