using Content.Server.Speech.Components;
using Robust.Shared.Random;
using System.Text;

namespace Content.Server.Speech.EntitySystems
{
    public sealed class WingdingsSpeechSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<WingdingsAccentComponent, AccentGetEvent>(OnAccent);
        }

        private static readonly IReadOnlyDictionary<char, char> WingdingsMap = new Dictionary<char, char>()
        {
            { 'а', '✌' }, { 'б', '☝' }, { 'в', '✍' }, { 'г', '☠' }, { 'д', '⚡' },
            { 'е', '☢' }, { 'ё', '☣' }, { 'ж', '☮' }, { 'з', '☯' }, { 'и', '♻' },
            { 'й', '⚛' }, { 'к', '♿' }, { 'л', '⚧' }, { 'м', '⚕' }, { 'н', '⚖' },
            { 'о', '⚗' }, { 'п', '⚙' }, { 'р', '⚚' }, { 'с', '⚜' }, { 'т', '⚝' },
            { 'у', '⚞' }, { 'ф', '⚟' }, { 'х', '⚠' }, { 'ц', '⚡' }, { 'ч', '☠' },
            { 'ш', '☢' }, { 'щ', '☣' }, { 'ъ', '☮' }, { 'ы', '☯' }, { 'ь', '♻' },
            { 'э', '⚛' }, { 'ю', '♿' }, { 'я', '⚧' },
            { 'a', '✌' }, { 'b', '☝' }, { 'c', '✍' }, { 'd', '☠' }, { 'e', '⚡' },
            { 'f', '☢' }, { 'g', '☣' }, { 'h', '☮' }, { 'i', '☯' }, { 'j', '♻' },
            { 'k', '⚛' }, { 'l', '♿' }, { 'm', '⚧' }, { 'n', '⚕' }, { 'o', '⚖' },
            { 'p', '⚗' }, { 'q', '⚙' }, { 'r', '⚚' }, { 's', '⚜' }, { 't', '⚝' },
            { 'u', '⚞' }, { 'v', '⚟' }, { 'w', '⚠' }, { 'x', '⚡' }, { 'y', '☠' },
            { 'z', '☢' }
        };

        public string ConvertToWingdings(string message)
        {
            var result = new StringBuilder();

            foreach (var character in message)
            {
                if (WingdingsMap.TryGetValue(char.ToLower(character), out var wingdingsChar))
                {
                    result.Append(wingdingsChar);
                }
                else
                {
                    result.Append(character);
                }
            }

            return result.ToString();
        }

        private void OnAccent(EntityUid uid, WingdingsAccentComponent component, AccentGetEvent args)
        {
            args.Message = ConvertToWingdings(args.Message);
        }
    }
}
