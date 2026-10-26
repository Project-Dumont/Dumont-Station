using Content.Shared._Dumont.Curse.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Dumont.Curse;

public sealed class CurseCavityBombSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;

    private static readonly ProtoId<TagPrototype> CurseBombTag = "CurseBomb";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseCavityBombComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<CurseCavityBombComponent> ent, ref ComponentStartup args)
    {
        PlantBomb(ent);
        RemCompDeferred<CurseCavityBombComponent>(ent);
    }

    private void PlantBomb(Entity<CurseCavityBombComponent> ent)
    {
        foreach (var (chest, part) in _body.GetBodyChildrenOfType(ent, BodyPartType.Chest))
        {
            var proto = _random.Prob(ent.Comp.RealChance) ? ent.Comp.RealPrototype : ent.Comp.FakePrototype;
            var bomb = Spawn(proto, Transform(ent).Coordinates);

            if (!_itemSlots.TryInsert(chest, part.ItemInsertionSlot, bomb, null))
            {
                Del(bomb);
                return;
            }

            _trigger.ActivateTimerTrigger(bomb, ent);
            _popup.PopupEntity(Loc.GetString("curse-bomb-popup"), ent, ent, PopupType.MediumCaution);
            return;
        }
    }

    public void DisarmBombs(EntityUid uid)
    {
        foreach (var (_, part) in _body.GetBodyChildrenOfType(uid, BodyPartType.Chest))
            if (part.ItemInsertionSlot.Item is { } bomb && _tag.HasTag(bomb, CurseBombTag))
                _trigger.StopTimerTrigger(bomb);
    }
}
