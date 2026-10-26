using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.FaceHugger;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Inventory;
using Content.Shared._White.Xenomorphs.Infection;
using Content.Shared._White.Xenomorphs.Larva;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Map;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphEggMorpherChildRoleComponent : Component
{
    public EntityUid Morpher;
}

public sealed partial class XenomorphEggMorpherSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private GhostRoleSystem _ghost = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private FaceHuggerSystem _face = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    private readonly Dictionary<EntityUid, EntityUid> _roles = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphEggMorpherComponent, InteractHandEvent>(OnHand);
        SubscribeLocalEvent<XenomorphEggMorpherComponent, InteractUsingEvent>(OnUsing);
        SubscribeLocalEvent<XenomorphEggMorpherComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<XenomorphEggMorpherComponent, GetVerbsEvent<ActivationVerb>>(OnVerbs);
        SubscribeLocalEvent<XenomorphEggMorpherComponent, XenomorphCarrierReserveMessage>(OnReserve);
        SubscribeLocalEvent<XenomorphEggMorpherComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphEggMorpherChildRoleComponent, TakeGhostRoleEvent>(OnTakeRole);
    }

    private bool CanUse(EntityUid user, EntityUid morpher) => HasComp<XenomorphLineageComponent>(user)
        && HasComp<XenomorphComponent>(user) && _mobs.IsAlive(user) && _interaction.InRangeUnobstructed(user, morpher);

    private void Changed(Entity<XenomorphEggMorpherComponent> ent)
    {
        Dirty(ent);
        _appearance.SetData(ent, EggmorpherOverlayVisuals.Number, ent.Comp.CurParasites);
    }

    private bool CanStore(EntityUid parasite) => !TerminatingOrDeleted(parasite) && !EntityManager.IsQueuedForDeletion(parasite) && HasComp<FaceHuggerComponent>(parasite) && _mobs.IsAlive(parasite)
        && (!TryComp<MindContainerComponent>(parasite, out var mind) || !mind.HasMind)
        && (!TryComp<ClothingComponent>(parasite, out var clothing) || clothing.InSlot == null);

    private void OnHand(Entity<XenomorphEggMorpherComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !CanUse(args.User, ent)) return;
        args.Handled = true;
        if (ent.Comp.CurParasites == 0)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-hive-morpher-empty"), ent, args.User);
            return;
        }
        var parasite = Spawn(XenomorphEggMorpherComponent.ParasitePrototype, _transform.GetMoverCoordinates(ent));
        var face = Comp<FaceHuggerComponent>(parasite);
        face.Active = false;
        face.RestIn = _timing.CurTime + TimeSpan.FromSeconds(1);
        EnsureComp<XenomorphCarrierRecoveryComponent>(parasite).RecoverAt = face.RestIn;
        _hands.TryPickupAnyHand(args.User, parasite);
        ent.Comp.CurParasites--;
        Changed(ent);
    }

    private void OnUsing(Entity<XenomorphEggMorpherComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !CanUse(args.User, ent) || !CanStore(args.Used)) return;
        args.Handled = true;
        if (ent.Comp.CurParasites >= ent.Comp.MaxParasites)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-hive-morpher-full"), ent, args.User);
            return;
        }
        QueueDel(args.Used);
        ent.Comp.CurParasites++;
        Changed(ent);
    }

    private void OnExamine(Entity<XenomorphEggMorpherComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<XenomorphComponent>(args.Examiner))
            args.PushMarkup(Loc.GetString("xenomorph-hive-morpher-count", ("count", ent.Comp.CurParasites), ("max", ent.Comp.MaxParasites)));
    }

    private void OnVerbs(Entity<XenomorphEggMorpherComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !CanUse(args.User, ent)) return;
        var user = args.User;
        args.Verbs.Add(new ActivationVerb
        {
            Text = Loc.GetString("xenomorph-hive-reserve-parasites"),
            Act = () => _ui.TryOpenUi(ent.Owner, XenomorphCarrierUiKey.Reserve, user),
        });
    }

    private void OnReserve(Entity<XenomorphEggMorpherComponent> ent, ref XenomorphCarrierReserveMessage args)
    {
        if (!CanUse(args.Actor, ent)) return;
        ent.Comp.ReservedParasites = Math.Clamp(args.Count, 0, ent.Comp.MaxParasites);
        Changed(ent);
    }

    private void OnTakeRole(Entity<XenomorphEggMorpherChildRoleComponent> ent, ref TakeGhostRoleEvent args)
    {
        if (TerminatingOrDeleted(ent.Comp.Morpher) || !TryComp<XenomorphEggMorpherComponent>(ent.Comp.Morpher, out var morpher)
            || morpher.CurParasites <= morpher.ReservedParasites) return;
        var parasite = Spawn(XenomorphEggMorpherComponent.ParasitePrototype, _transform.GetMoverCoordinates(ent.Comp.Morpher));
        morpher.CurParasites--;
        Changed((ent.Comp.Morpher, morpher));
        _mind.MakeSentient(parasite, true, true);
        _ghost.GhostRoleInternalCreateMindAndTransfer(args.Player, ent, parasite);
        args.TookRole = true;
    }

    private void OnShutdown(Entity<XenomorphEggMorpherComponent> ent, ref ComponentShutdown args)
    {
        if (_roles.Remove(ent, out var role) && Exists(role)) QueueDel(role);
    }

    public override void Update(float frameTime)
    {
        var queenOving = false;
        var queens = EntityQueryEnumerator<XenomorphOvipositorComponent>();
        while (queens.MoveNext(out var uid, out var queen))
            if (queen.Attached && _mobs.IsAlive(uid)) { queenOving = true; break; }
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<XenomorphEggMorpherComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (TerminatingOrDeleted(uid)) continue;
            if (comp.CurParasites < comp.GrowMaxParasites)
            {
                var next = now + (queenOving ? comp.OviSpawnCooldown : comp.StandardSpawnCooldown);
                if (comp.NextSpawnAt == null || next < comp.NextSpawnAt) comp.NextSpawnAt = next;
                if (comp.NextSpawnAt <= now)
                {
                    comp.CurParasites++;
                    comp.NextSpawnAt = next;
                    Changed((uid, comp));
                }
            }
            if (comp.CurParasites > 0 && now >= comp.NextTrigger)
            {
                comp.NextTrigger = now + TimeSpan.FromSeconds(1);
                foreach (var target in _lookup.GetEntitiesInRange<InventoryComponent>(Transform(uid).Coordinates, 1.5f))
                {
                    if (!_mobs.IsAlive(target) || HasComp<XenomorphComponent>(target) || HasComp<XenomorphInfectedComponent>(target)
                        || HasComp<XenomorphLarvaVictimComponent>(target) || !_interaction.InRangeUnobstructed(uid, target.Owner, 1.5f)) continue;
                    var parasite = Spawn(XenomorphEggMorpherComponent.ParasitePrototype, _transform.GetMoverCoordinates(uid));
                    if (_face.TryEquipFaceHugger(parasite, target, Comp<FaceHuggerComponent>(parasite)))
                    {
                        comp.CurParasites--;
                        Changed((uid, comp));
                    }
                    else QueueDel(parasite);
                    break;
                }
            }
            if (comp.CurParasites > comp.ReservedParasites)
            {
                if (!_roles.TryGetValue(uid, out var role) || !Exists(role))
                {
                    role = Spawn("DumontHiveEggMorpherChildRole", new EntityCoordinates(uid, Vector2.Zero));
                    Comp<XenomorphEggMorpherChildRoleComponent>(role).Morpher = uid;
                    _roles[uid] = role;
                }
            }
            else if (_roles.Remove(uid, out var role) && Exists(role)) QueueDel(role);
        }
    }
}
