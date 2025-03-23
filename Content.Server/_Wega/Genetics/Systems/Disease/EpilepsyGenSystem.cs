using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Genetics;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Genetics.System;

public sealed class EpilepsySystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedJitteringSystem _jitteringSystem = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EpilepsyGenComponent>();
        while (query.MoveNext(out var uid, out var epilepsy))
        {
            if (epilepsy.NextTimeTick <= 0)
            {
                epilepsy.NextTimeTick = 10;
                if (_random.Next(0, 100) < 1)
                {
                    _stun.TryParalyze(uid, TimeSpan.FromSeconds(15), true);
                    _jitteringSystem.DoJitter(uid, TimeSpan.FromSeconds(15), true);
                    _popup.PopupClient(Loc.GetString("disease-epilepsy-massage"), uid, PopupType.Medium);
                    _chat.TryEmoteWithoutChat(uid, _prototypeManager.Index<EmotePrototype>("Scream"), true);
                }
            }
            epilepsy.NextTimeTick -= frameTime;
        }
    }
}

