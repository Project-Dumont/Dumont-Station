// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.LeaderForm;

public sealed class BloodCultLeaderFormSystem : EntitySystem
{
    private const string SuitSlot = "outerClothing";
    private const string HeadSlot = "head";

    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PentagramComponent, ComponentStartup>(OnPentagramStartup);
        SubscribeLocalEvent<PentagramComponent, ComponentShutdown>(OnPentagramShutdown);
        SubscribeLocalEvent<BloodCultLeaderComponent, ComponentInit>(OnLeaderInit);
        SubscribeLocalEvent<BloodCultLeaderComponent, ComponentRemove>(OnLeaderRemove);

        SubscribeLocalEvent<BloodCultLeaderFormComponent, ComponentShutdown>(OnFormShutdown);
    }

    private void OnPentagramStartup(Entity<PentagramComponent> ent, ref ComponentStartup args) => Refresh(ent);

    private void OnPentagramShutdown(Entity<PentagramComponent> ent, ref ComponentShutdown args) => Clear(ent);

    private void OnLeaderInit(Entity<BloodCultLeaderComponent> ent, ref ComponentInit args) => Refresh(ent);

    private void OnLeaderRemove(Entity<BloodCultLeaderComponent> ent, ref ComponentRemove args) => Clear(ent);

    private void Refresh(EntityUid uid)
    {
        if (TerminatingOrDeleted(uid) || HasComp<BloodCultLeaderFormComponent>(uid))
            return;

        if (!HasComp<BloodCultLeaderComponent>(uid) || !HasComp<PentagramComponent>(uid))
            return;

        var comp = AddComp<BloodCultLeaderFormComponent>(uid);

        comp.Suit = Dress(uid, comp.SuitProto, SuitSlot);
        comp.Head = Dress(uid, comp.HeadProto, HeadSlot);
    }

    private EntityUid? Dress(EntityUid uid, EntProtoId proto, string slot)
    {
        _inventory.TryUnequip(uid, slot, force: true);

        var piece = Spawn(proto, Transform(uid).Coordinates);

        if (_inventory.TryEquip(uid, piece, slot, silent: true, force: true))
            return piece;

        QueueDel(piece);
        return null;
    }

    private void Clear(EntityUid uid)
    {
        if (TerminatingOrDeleted(uid))
            return;

        RemComp<BloodCultLeaderFormComponent>(uid);
    }

    private void OnFormShutdown(Entity<BloodCultLeaderFormComponent> ent, ref ComponentShutdown args)
    {
        Undress(ent, ent.Comp.Suit, SuitSlot);
        Undress(ent, ent.Comp.Head, HeadSlot);

        ent.Comp.Suit = null;
        ent.Comp.Head = null;
    }

    private void Undress(EntityUid uid, EntityUid? piece, string slot)
    {
        if (piece is not { } item)
            return;

        RemComp<UnremoveableComponent>(item);

        if (!TerminatingOrDeleted(uid))
            _inventory.TryUnequip(uid, slot, force: true);

        QueueDel(item);
    }
}
