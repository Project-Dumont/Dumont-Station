using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Text;
using System.Text.RegularExpressions;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class GibberishSpeechSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<AphasiaAccentComponent, AccentGetEvent>(OnAccent);
        }

        private static readonly Regex WordSplitRegex = new Regex(@"(\W+)", RegexOptions.Compiled);

        private static readonly IReadOnlyList<string> GibberishSyllables = new List<string>
        {
            "meh", "nah", "bleh", "blah", "foo", "bar", "baz", "qux", "zog", "glorp",
            "flib", "flob", "zib", "zab", "grok", "snarf", "blip", "blop", "plop", "splort",
            "wibble", "wobble", "dib", "dab", "fizz", "buzz", "zap", "zorp", "flap", "flop",

            "blorpt", "snizzle", "flibber", "glabber", "zizzle", "wizzle", "drizzle", "frizzle",
            "sproing", "sploosh", "blargle", "glurp", "snork", "blump", "flump", "zump",
            "wump", "doodle", "noodle", "poodle", "squiggle", "wiggle", "giggle", "snuggle",

            "boing", "splat", "whoosh", "thud", "clang", "bam", "pow", "zap", "wham", "zoom",
            "vroom", "swoosh", "ding", "dong", "clink", "clank", "clunk", "thump", "crash", "bang"
        }.AsReadOnly();

        public string ConvertToGibberish(string message)
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

                var gibberishWord = MakeGibberish(word);
                result.Append(gibberishWord);
            }

            return result.ToString();
        }

        private string MakeGibberish(string word)
        {
            var gibberishWord = new StringBuilder();

            if (_random.Prob(0.5f))
            {
                gibberishWord.Append(_random.Pick(GibberishSyllables));
            }

            for (int i = 0; i < word.Length; i++)
            {
                if (_random.Prob(0.3f))
                {
                    gibberishWord.Append((char)_random.Next('a', 'z' + 1));
                }
                else
                {
                    gibberishWord.Append(word[i]);
                }
            }

            if (_random.Prob(0.5f))
            {
                gibberishWord.Append(_random.Pick(GibberishSyllables));
            }

            return gibberishWord.ToString();
        }

        private void OnAccent(EntityUid uid, AphasiaAccentComponent component, AccentGetEvent args)
        {
            args.Message = ConvertToGibberish(args.Message);
        }
    }
}
