using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Caste;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphHiveRanksSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private XenomorphHiveCaptureSystem _hive = default!;
    [Dependency] private IGameTiming _timing = default!;
    private TimeSpan? _queenReplacementAt;
    private bool _extraGuardUnlocked;
    private EntityUid? _queen;
    public bool QueenHasExisted { get; private set; }
    public bool QueenFallen { get; private set; }
    public int SoldierLimit => _extraGuardUnlocked ? 6 : 4;
    public int RoyalGuardLimit => _extraGuardUnlocked ? 2 : 1;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestart);
        SubscribeLocalEvent<XenomorphLineageComponent, MapInitEvent>(OnSpawned);
        SubscribeLocalEvent<XenomorphLineageComponent, MobStateChangedEvent>(OnState);
        SubscribeLocalEvent<XenomorphLineageComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<XenomorphLineageComponent, ComponentShutdown>(OnRemoved);
    }

    private void OnRestart(RoundRestartCleanupEvent args)
    {
        _extraGuardUnlocked = false;
        _queen = null;
        _queenReplacementAt = null;
        QueenHasExisted = false;
        QueenFallen = false;
    }

    private void OnSpawned(Entity<XenomorphLineageComponent> ent, ref MapInitEvent args)
    {
        if (!HasComp<XenomorphRoyalComponent>(ent) || (QueenHasExisted && !QueenFallen)) return;
        QueenHasExisted = true;
        QueenFallen = false;
        _queenReplacementAt = null;
        _queen = ent;
    }

    private void OnState(Entity<XenomorphLineageComponent> ent, ref MobStateChangedEvent args)
    {
        if (ent.Owner != _queen || QueenFallen || args.NewMobState != MobState.Dead) return;
        QueenFallen = true;
        _queenReplacementAt = _timing.CurTime + TimeSpan.FromMinutes(10);
        _hive.AnnounceHive(Loc.GetString("xenomorph-queen-fallen"),
            new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_died.ogg", AudioParams.Default.WithVolume(-6)));
    }

    public bool UnlockRoyalGuardSlot()
    {
        if (_extraGuardUnlocked) return false;
        _extraGuardUnlocked = true;
        return true;
    }

    private void OnRemoved(Entity<XenomorphLineageComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner != _queen || QueenFallen) return;
        QueenFallen = true;
        _queenReplacementAt = _timing.CurTime + TimeSpan.FromMinutes(10);
        _hive.AnnounceHive(Loc.GetString("xenomorph-queen-fallen"),
            new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_died.ogg", AudioParams.Default.WithVolume(-6)));
    }

    public int Tier(ProtoId<XenomorphCastePrototype> caste) => _prototypes.Index(caste).Tier;

    public bool CanEvolve(EntityUid user, ProtoId<XenomorphCastePrototype> caste, bool popup = true)
    {
        if (!HasComp<XenomorphLineageComponent>(user)) return true;
        string? reason = null;
        var limit = RoyalGuardLimit;
        if (caste == "Queen" && QueenHasExisted)
        {
            if (!QueenFallen)
                reason = "xenomorph-queen-already-alive";
            else if (_queenReplacementAt is { } replacement && _timing.CurTime < replacement)
                reason = "xenomorph-queen-replacement-wait";
        }
        else if (Tier(caste) is 2 or 3)
        {
            var tier = Tier(caste);
            limit = tier == 2 ? SoldierLimit : RoyalGuardLimit;
            var count = 0;
            var query = EntityQueryEnumerator<XenomorphLineageComponent, XenomorphComponent>();
            while (query.MoveNext(out var uid, out _, out var xeno))
                if (uid != user && !TerminatingOrDeleted(uid) && !_mobs.IsDead(uid) && Tier(xeno.Caste) == tier) count++;
            if (count >= limit) reason = tier == 2 ? "xenomorph-hive-soldier-limit" : "xenomorph-royal-guard-limit";
        }
        if (reason == null) return true;
        if (popup)
        {
            var seconds = Math.Max(0, (int) Math.Ceiling(((_queenReplacementAt ?? _timing.CurTime) - _timing.CurTime).TotalSeconds));
            _popup.PopupEntity(Loc.GetString(reason, ("limit", limit), ("seconds", seconds)), user, user);
        }
        return false;
    }

    private void OnExamined(Entity<XenomorphLineageComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<XenomorphComponent>(ent, out var xeno)) return;
        var tier = Tier(xeno.Caste);
        if (tier is >= 1 and <= 3) args.PushMarkup(Loc.GetString("xenomorph-rank-examine", ("rank", Loc.GetString($"xenomorph-rank-{tier}"))));
    }
}
