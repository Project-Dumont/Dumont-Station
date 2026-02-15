using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Genetics;
using Content.Shared.Jittering;
using Content.Shared.Stunnable;
using Robust.Shared.Random;

namespace Content.Server.Genetics.System;

public sealed class TourettesSyndromeSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedJitteringSystem _jitteringSystem = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly IReadOnlyList<string> SwearWords = new List<string>
    {
        "porra", "caralho", "merda", "foda", "cacete", "puta", "buceta", "cu", "corno", "viado",
        "porra!", "caralho!", "merda!", "foda!", "cacete!", "puta!", "buceta!", "cu!", "corno!", "viado!"
    }.AsReadOnly();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TourettesSyndromeComponent>();
        while (query.MoveNext(out var uid, out var tourettes))
        {
            if (tourettes.NextTimeTick <= 0)
            {
                tourettes.NextTimeTick = 35;

                var swearWord = _random.Pick(SwearWords);
                _jitteringSystem.DoJitter(uid, TimeSpan.FromSeconds(8), true);
                _chat.TrySendInGameICMessage(uid, swearWord, InGameICChatType.Speak, false);
                if (_random.Next(0, 100) < 10)
                {
                    _stun.TryUpdateStunDuration(uid, TimeSpan.FromSeconds(_random.Next(1, 31)));
                }
            }
            tourettes.NextTimeTick -= frameTime;
        }
    }
}

