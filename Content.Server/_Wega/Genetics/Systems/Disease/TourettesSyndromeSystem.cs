using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Genetics;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Genetics.System;

public sealed class TourettesSyndromeSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedJitteringSystem _jitteringSystem = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly IReadOnlyList<string> SwearWords = new List<string>
    {
        "бля", "ёпт", "нах", "пизда", "хуй", "ебать", "сука", "гандон", "мудак", "долбоёб",
        "бля!", "ёпт!", "нах!", "пизда!", "хуй!", "ебать!", "сука!", "гандон!", "мудак!", "долбоёб!"
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
                    _stun.TryStun(uid, TimeSpan.FromSeconds(_random.Next(1, 31)), true);
                }
            }
            tourettes.NextTimeTick -= frameTime;
        }
    }
}

