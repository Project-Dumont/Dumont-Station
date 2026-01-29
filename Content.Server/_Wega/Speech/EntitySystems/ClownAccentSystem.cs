using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class ClownAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<ClownAccentComponent, AccentGetEvent>(OnAccent);
        }

        private static readonly IReadOnlyList<string> LaughsAndExclamations = new List<string>
        {
            " Opa!", " Ha ha!", " He he!", " Buá há há!", " Ho ho!", " Uau!", " Bam!", " Boom!"
        }.AsReadOnly();

        private static readonly IReadOnlyDictionary<string, string> SpecialWords = new Dictionary<string, string>()
        {
            { "oi", "buzina" },
            { "ola", "buzina" },
            { "tchau", "buzina-buzina" },
            { "adeus", "buzina-buzina" },
            { "sim", "aham-aham" },
            { "não", "nein" },
            { "muito", "muito muito" },
            { "grande", "gigantesco" },
            { "pequeno", "pequenininho" },
            { "engraçado", "hilário" },
        };

        public string Accentuate(string message)
        {
            foreach (var (word, repl) in SpecialWords)
            {
                message = message.Replace(word, repl, StringComparison.OrdinalIgnoreCase);
            }

            if (_random.Prob(0.5f))
            {
                message += _random.Pick(LaughsAndExclamations);
            }

            return message;
        }

        private void OnAccent(EntityUid uid, ClownAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
