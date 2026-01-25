// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Content.Shared.Contraband;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.CriminalRecords;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Roles;
using Content.Shared.Security.Components;
using Robust.Shared.Prototypes;
using System.Linq;
using Robust.Shared.GameObjects;
using Content.Shared.PDA;
using Robust.Shared.Timing;

namespace Content.Shared.Security.Systems;

public sealed class CriminalStatusSystem : EntitySystem
{
    [Dependency] private readonly SharedAccessSystem _access = default!;
    [Dependency] private readonly SharedIdCardSystem _id = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const int Delay = 5; // Seconds
    private const int PrivilegedDelay = 5; // Minutes

    private static readonly HashSet<ProtoId<DepartmentPrototype>> PrivilegedDepartments = new()
    {
        "CentralCommand", "Command", "Security"
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CriminalRecordComponent, CriminalRecordChanged>(OnCriminalRecordChanged);

        SubscribeLocalEvent<CriminalRecordComponent, ClothingDidEquippedEvent>((u, c, a) => OnEquippedOrUniquip(u, c, true, a.Clothing.Owner, a.Clothing.Comp));
        SubscribeLocalEvent<CriminalRecordComponent, ClothingDidUnequippedEvent>((u, c, a) => OnEquippedOrUniquip(u, c, false, a.Clothing.Owner, a.Clothing.Comp));

        SubscribeLocalEvent<CriminalRecordComponent, DidEquipHandEvent>((u, c, a) => OnPickupOrDrop(u, c, a.Equipped, true));
        SubscribeLocalEvent<CriminalRecordComponent, DidUnequipHandEvent>((u, c, a) => OnPickupOrDrop(u, c, a.Unequipped, false));

        SubscribeLocalEvent<PdaComponent, PdaIdChangedEvent>(UpdatePdaId);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var removePrivileged = new List<EntityUid>();
        var query = EntityQueryEnumerator<PrivilegedStatusComponent>();
        while (query.MoveNext(out var uid, out var status))
        {
            if (status.PrivilegedTime != TimeSpan.Zero && _timing.CurTime > status.PrivilegedTime)
            {
                removePrivileged.Add(uid);
            }
        }

        foreach (var uid in removePrivileged)
        {
            RemComp<PrivilegedStatusComponent>(uid);

            if (TryComp<CriminalRecordComponent>(uid, out var record))
                RecalculatePoints((uid, record));
        }
    }

    private void OnCriminalRecordChanged(EntityUid uid, CriminalRecordComponent component, CriminalRecordChanged args)
    {
        component.Points -= component.SecurityStatusPoints[args.PreviousStatus];
        component.Points += component.SecurityStatusPoints[args.Status];
    }

    private bool OnEquippedOrUniquip(EntityUid uid, CriminalRecordComponent component, bool equip, EntityUid clothingUid, ClothingComponent? clothingComp = null, bool checkId = true)
    {
        if (!Resolve(clothingUid, ref clothingComp, false))
            return false;

        if (clothingComp.InSlot == null)
            return false;

        if (checkId && UpdateIdCard((uid, component), clothingUid))
            return true;

        if (!TryComp<ContrabandComponent>(clothingUid, out var contraband))
            return true;

        if (contraband.CriminalPoints == 0f)
            return true;

        if (!_inventory.TryGetSlots(uid, out var slots))
            return true;

        SlotFlags? slot = null;

        foreach (var invSlot in slots)
        {
            if (clothingComp.InSlot != invSlot.Name)
                continue;

            slot = invSlot.SlotFlags;
            break;
        }

        if (slot == null)
            return true;

        if (!component.ClothingSlotPoints.TryGetValue(slot.Value, out var slotMultiplier))
            return true;

        if (CheckIdCard(uid, contraband))
            return true;

        var points = GetPoints(clothingUid, contraband.CriminalPoints);

        if (equip)
            component.Points += points;
        else
            PointDelay(uid, points);

        return true;
    }

    private bool OnPickupOrDrop(EntityUid uid, CriminalRecordComponent component, EntityUid item, bool pickup, bool checkId = true)
    {
        if (checkId && UpdateIdCard((uid, component), item))
            return true;

        if (!TryComp<ContrabandComponent>(item, out var contraband))
            return false;

        if (contraband.CriminalPoints == 0f)
            return true;

        if (CheckIdCard(uid, contraband))
            return true;

        var points = GetPoints(item, contraband.CriminalPoints);

        if (pickup)
            component.Points += points;
        else
            PointDelay(uid, points);

        return true;
    }

    private bool CheckIdCard(EntityUid uid, ContrabandComponent contraband)
    {
        if (HasComp<PrivilegedStatusComponent>(uid))
            return true;

        if (!_id.TryFindIdCards(uid, out var ids))
            return false;

        var allowedJobNames = contraband.AllowedJobs
            .Select(p => _prototype.Index(p).LocalizedName)
            .ToHashSet();

        foreach (var id in ids)
        {
            var cardComp = id.Comp;

            if (cardComp.JobDepartments.Intersect(contraband.AllowedDepartments).Any())
                return true;

            if (cardComp.LocalizedJobTitle is not null && allowedJobNames.Contains(cardComp.LocalizedJobTitle))
                return true;
        }

        return false;
    }

    private bool UpdateIdCard(Entity<CriminalRecordComponent> ent, EntityUid item)
    {
        // if access has changed we need to recalculate
        if (!_id.TryGetIdCard(item, out _))
            return false;

        UpdatePrivilegedStatus(ent.Owner);
        RecalculatePoints(ent);
        return true;
    }

    private void UpdatePdaId(EntityUid uid, PdaComponent component, PdaIdChangedEvent args)
    {
        var parent = Transform(uid).ParentUid;
        if (TryComp<CriminalRecordComponent>(parent, out var criminalRecord))
        {
            UpdatePrivilegedStatus(parent);
            RecalculatePoints((parent, criminalRecord));
        }
    }

    private float GetPoints(EntityUid uid, float points)
    {
        var ev = new GetCriminalPointsEvent(points);
        RaiseLocalEvent(uid, ev);

        return ev.Points;
    }

    private void RecalculatePoints(Entity<CriminalRecordComponent> ent)
    {
        ent.Comp.Points = 0f;

        foreach (var item in _inventory.GetHandOrInventoryEntities(ent.Owner))
        {
            if (OnEquippedOrUniquip(ent.Owner, ent.Comp, true, item, checkId: false))
                continue;

            OnPickupOrDrop(ent.Owner, ent.Comp, item, true, checkId: false);
        }
    }

    private void PointDelay(EntityUid uid, float points)
    {
        Timer.Spawn(TimeSpan.FromSeconds(Delay), () =>
        {
            if (TryComp<CriminalRecordComponent>(uid, out var currentRecord))
                currentRecord.Points = Math.Max(0, currentRecord.Points - points);
        });
    }

    private void UpdatePrivilegedStatus(EntityUid uid)
    {
        bool privageled = false;

        if (_id.TryFindIdCards(uid, out var ids))
        {
            foreach (var id in ids)
            {
                if (id.Comp.JobDepartments.Intersect(PrivilegedDepartments).Any())
                {
                    privageled = true;
                    break;
                }
            }
        }

        if (privageled)
        {
            var comp = EnsureComp<PrivilegedStatusComponent>(uid);
            if (comp.PrivilegedTime != TimeSpan.Zero)
            {
                comp.PrivilegedTime = TimeSpan.Zero;
                Dirty(uid, comp);
            }
        }
        else
        {
            if (TryComp<PrivilegedStatusComponent>(uid, out var comp))
            {
                if (comp.PrivilegedTime == TimeSpan.Zero)
                {
                    comp.PrivilegedTime = _timing.CurTime + TimeSpan.FromMinutes(PrivilegedDelay);
                    Dirty(uid, comp);
                }
            }
        }
    }
}

public sealed class PdaIdChangedEvent : EntityEventArgs
{
    public EntityUid PdaUid;
    public EntityUid? IdUid;

    public PdaIdChangedEvent(EntityUid pdaUid, EntityUid? idUid)
    {
        PdaUid = pdaUid;
        IdUid = idUid;
    }
}
