

namespace Content.Shared._Gabystation.NanoBank;

public sealed class NanoBankCardSystem : EntitySystem
{
    //[Dependency] private readonly EconomyManagerSystem _economy = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    /*public bool TryGetCardsWithAccount(int id, out AllEntityQueryEnumerator<NanoBankCardComponent> cards)
    {
        cards = new AllEntityQueryEnumerator<NanoBankCardComponent>();

        var ents = AllEntityQuery<NanoBankCardComponent>();
        while (ents.MoveNext(out var uid, out var comp))
        {
            if (!comp.LoggedIn || comp.AccountId is null || comp.AccountPin is null)
                continue;

        }

        return true;
    }*/
}
