using Content.Shared._Dumont.Curse.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Curse;

public sealed class CurseDropItemSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseDropItemComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CurseDropItemComponent> ent, ref MapInitEvent args)
    {
        Schedule(ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<CurseDropItemComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextDrop)
                continue;

            Schedule(comp);

            if (_hands.GetActiveItem(uid) is not { } item || !_hands.TryDrop(uid, item))
                continue;

            _popup.PopupEntity(Loc.GetString("curse-drop-popup", ("item", item)), uid, uid, PopupType.SmallCaution);
        }
    }

    private void Schedule(CurseDropItemComponent comp)
    {
        comp.NextDrop = _timing.CurTime + _random.Next(comp.MinInterval, comp.MaxInterval);
    }
}
