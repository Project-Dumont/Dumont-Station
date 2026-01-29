using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class UnintelligibleAccentSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        private static readonly Regex WordSplitRegex = new Regex(@"(\W+)", RegexOptions.Compiled);
        public override void Initialize()
        {
            SubscribeLocalEvent<UnintelligibleAccentComponent, AccentGetEvent>(OnAccent);
        }

        public string JumbleSpeech(string message)
        {
            var words = WordSplitRegex.Split(message);
            var result = new StringBuilder();

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word) || word.Length <= 3)
                {
                    result.Append(word);
                    continue;
                }

                var jumbledWord = JumbleWord(word);
                result.Append(jumbledWord);
            }

            return result.ToString();
        }

        private string JumbleWord(string word)
        {
            var parts = new List<string>();

            for (int i = 0; i < word.Length; i += _random.Next(2, 4))
            {
                var part = word.Substring(i, Math.Min(2, word.Length - i));
                parts.Add(part);
            }

            parts = parts.OrderBy(_ => _random.Next()).ToList();

            return string.Join("", parts);
        }

        private void OnAccent(EntityUid uid, UnintelligibleAccentComponent component, AccentGetEvent args)
        {
            args.Message = JumbleSpeech(args.Message);
        }
    }
}
