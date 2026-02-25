using System.Linq;
using Content.Server.Speech.Components;
using System.Text;
using System.Text.RegularExpressions;
using Content.Shared.Speech;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class AuldImperialAccentSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<AuldImperialAccentComponent, AccentGetEvent>(OnAccent);
        }

        private static readonly Regex WordSplitRegex = new Regex(@"(\W+)", RegexOptions.Compiled);
        private static readonly IReadOnlyDictionary<string, string> SpecialWords = new Dictionary<string, string>()
        {
            { "ſ", "s" },
            { "S", "S" },
            { "i", "y" },
            { "I", "Y" },
            { "f", "ph" },
            { "F", "PH" },
            { "T", "TH" },
            { "t", "th" },
            { "r", "rh" },
            { "R", "RH" },
            { "u", "ü" },
            { "U", "Ü" },
        };
        private static readonly IReadOnlyList<char> HardConsonants = new List<char>()
        {
        };

        public string Accentuate(string message)
        {
            var words = WordSplitRegex.Split(message);
            var result = new StringBuilder();

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    result.Append(word);
                    continue;
                }

                var newWord = new StringBuilder(word);
                foreach (var (key, value) in SpecialWords)
                {
                    newWord.Replace(key, value);
                }

                if (newWord.Length > 0 && HardConsonants.Contains(newWord[^1]))
                {
                    newWord.Append('h');
                }

                result.Append(newWord.ToString());
            }

            return result.ToString();
        }

        private void OnAccent(EntityUid uid, AuldImperialAccentComponent component, AccentGetEvent args)
        {
            args.Message = Accentuate(args.Message);
        }
    }
}
