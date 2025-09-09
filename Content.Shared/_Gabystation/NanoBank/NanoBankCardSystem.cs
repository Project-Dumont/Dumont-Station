

using Content.Shared.Examine;

namespace Content.Shared._Gabystation.NanoBank;

public abstract class SharedNanoBankSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NanoBankCardComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<NanoBankCardComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!ent.Comp.LoggedIn)
            return;

        args.PushMarkup(Loc.GetString("nanobank-card-examine-logged"));
    }

    public bool GetNotificationsMuted(Entity<NanoBankCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        return card.Comp.NotificationsMuted;
    }
    public void SetNotificationsMuted(Entity<NanoBankCardComponent?> card, bool muted)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.NotificationsMuted = muted;
        Dirty(card);
    }

    public void SetAccountId(Entity<NanoBankCardComponent?> card, int accountId)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.AccountId = accountId;
        Dirty(card);
    }

    public void LogoutId(Entity<NanoBankCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.AccountId = 0;
        card.Comp.AccountPin = 0;
        card.Comp.LoggedIn = false;
        Dirty(card);
    }
}

