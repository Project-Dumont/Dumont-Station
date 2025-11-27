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
            { "е", "ѣ" },
            { "Е", "Ѣ" },
            { "ф", "ѳ" },
            { "Ф", "Ѳ" },
            { "и", "i" },
            { "И", "I" },
            { "ч", "чь" },
            { "Ч", "Чь" },
            { "щ", "щь" },
            { "Щ", "Щь" },
            { "ж", "жь" },
            { "Ж", "Жь" },
            { "ш", "шь" },
            { "Ш", "Шь" },
            { "ц", "ць" },
            { "Ц", "Ць" },
        };
        private static readonly IReadOnlyList<char> HardConsonants = new List<char>()
        {
            'г', 'Г', 'к', 'К', 'в', 'В', 'з', 'З', 'с', 'С', 'т', 'Т', 'д', 'Д',
            'б', 'Б', 'п', 'П', 'м', 'М', 'н', 'Н', 'л', 'Л', 'р', 'Р', 'х', 'Х'
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
                    newWord.Append('ъ');
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
