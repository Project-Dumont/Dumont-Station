using System.Linq;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared._FarHorizons.Salvage;
using Content.Shared._FarHorizons.Salvage.Components;
using Content.Shared.Chat;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Procedural;
using Content.Shared.Salvage.Expeditions;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._FarHorizons.Salvage;

[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseSalvageMissionObjectiveHandler
{
    public bool Initialized = false;

    private static readonly Exception _uninitializedException = new("BaseSalvageMissionObjective unitialized");

    private ISawmill? _sawmill;
    protected ISawmill Log =>
        Initialized ? _sawmill! : throw _uninitializedException;

    private IEntityManager? _entityManager;
    protected IEntityManager EntMan =>
        Initialized ? _entityManager! : throw _uninitializedException;

    private IPrototypeManager? _protoManager;
    protected IPrototypeManager ProtoMan => 
        Initialized ? _protoManager! : throw _uninitializedException;

    private System.Random? _random;
    protected System.Random Rand => 
        Initialized ? _random! : throw _uninitializedException;

    private SalvageMissionObjectivePrototype? _objective;
    protected SalvageMissionObjectivePrototype Objective => 
        Initialized ? _objective! : throw _uninitializedException;

    private ProtoId<SalvageDifficultyPrototype>? _difficulty;
    protected ProtoId<SalvageDifficultyPrototype> Difficulty => 
        Initialized ? _difficulty!.Value : throw _uninitializedException;

    private Dungeon? _dungeon;
    protected Dungeon Dungeon => 
        Initialized ? _dungeon! : throw _uninitializedException;

    private Entity<MapGridComponent>? _map;
    protected Entity<MapGridComponent> Map => 
        Initialized ? _map!.Value : throw _uninitializedException;

    private AnchorableSystem? _anchorable;
    protected AnchorableSystem AnchorableSys => 
        Initialized ? _anchorable! : throw _uninitializedException;

    private SharedMapSystem? _mapSys;
    protected SharedMapSystem MapSys => 
        Initialized ? _mapSys! : throw _uninitializedException;

    public virtual void Init(
        ISawmill sawmill,
        IEntityManager entMan, 
        IPrototypeManager protoMan, 
        System.Random random, 
        SalvageMissionObjectivePrototype objective, 
        AnchorableSystem anchorable, 
        SharedMapSystem mapSys, 
        ProtoId<SalvageDifficultyPrototype> difficulty,
        Dungeon dungeon, 
        Entity<MapGridComponent> map)
    {
        _sawmill = sawmill;
        _entityManager = entMan;
        _protoManager = protoMan;
        _random = random;
        _objective = objective;
        _difficulty = difficulty;
        _dungeon = dungeon;
        _map = map;
        _anchorable = anchorable;
        _mapSys = mapSys;
        Initialized = true;
    }

    public virtual void Shutdown()
    {
        Initialized = false;
        _sawmill = null;
        _entityManager = null;
        _protoManager = null;
        _random = null;
        _objective = null;
        _difficulty = null;
        _dungeon = null;
        _map = null;
        _anchorable = null;
        _mapSys = null;
    }

    public virtual void Run(
        ISawmill sawmill,
        IEntityManager entMan, 
        IPrototypeManager protoMan, 
        System.Random random, 
        SalvageMissionObjectivePrototype objective, 
        AnchorableSystem anchorable, 
        SharedMapSystem mapSys, 
        ProtoId<SalvageDifficultyPrototype> difficulty,
        Dungeon dungeon, 
        Entity<MapGridComponent> map)
    {
        sawmill.Info($"{this.GetType().Name} handler entered");
        Init(sawmill, entMan, protoMan, random, objective, anchorable, mapSys, difficulty, dungeon, map);
        OnMapCreated();
    }

    public virtual void Exit(EntityUid shuttle)
    {
        BeforeFTLFromMap(shuttle);
        Shutdown();
    }

    public abstract void OnMapCreated();

    public abstract void BeforeFTLToMap(EntityUid shuttle);
    public abstract void AFterFTLToMap(EntityUid shuttle);
    public abstract void BeforeFTLFromMap(EntityUid shuttle);

    public virtual string GetAnnouncement() =>
        !Initialized
            ? ""
            : Loc.GetString(
                Objective.Announcement,
                ("numTargets", Objective.NumTargets.GetValueOrDefault(Difficulty, 0)),
                ("bonusCap", Objective.BonusCap));

    protected EntityCoordinates? GetRandomEmptyTileInDungeon()
    {
        ValueList<DungeonRoom> availableRooms = [.. Dungeon.Rooms];
        List<Vector2i> availableTiles = [];

        while (availableRooms.Count > 0)
        {
            availableTiles.Clear();
            var roomIndex = Rand.Next(availableRooms.Count);
            var room = availableRooms.RemoveSwap(roomIndex);
            availableTiles.AddRange(room.Tiles);

            while (availableTiles.Count > 0)
            {
                var tile = availableTiles.RemoveSwap(Rand.Next(availableTiles.Count));

                if (!AnchorableSys.TileFree(Map, tile, (int)CollisionGroup.MachineLayer,
                        (int)CollisionGroup.MachineLayer))
                    continue;

                return MapSys.GridTileToLocal(Map, Map.Comp, tile);
            }
        }

        return null;
    }

    protected HashSet<EntityUid> GetAllSpawnedMobs()
    {
        if (!EntMan.TryGetComponent<TransformComponent>(Map, out var mapTransform))
            return [];

        HashSet<EntityUid> result = [];
        var enumerator = mapTransform.ChildEnumerator;
        while(enumerator.MoveNext(out var uid))
        {
            if(EntMan.TryGetComponent<MobStateComponent>(uid, out _))
                result.Add(uid);
        }
        return result;
    }

    protected void Announce(string text)
    {
        if (!EntMan.TryGetComponent<MapComponent>(Map, out var mapComp))
            return;

        var chat = IoCManager.Resolve<IChatManager>();

        chat.ChatMessageToManyFiltered(
            Filter.BroadcastMap(mapComp.MapId),
            ChatChannel.Radio,
            text,
            text,
            Map,
            false,
            true,
            null);
    }

    protected void SetRewardComponent(EntityUid target, (bool completed, int bonuses, int maxBonuses, int totalReward) reward)
    {
        var comp = EntMan.EnsureComponent<SalvageMissionRewardComponent>(target);
        comp.MissionCompleted = reward.completed;
        comp.Bonuses = reward.bonuses;
        comp.MaxBonuses = reward.maxBonuses;
        comp.TotalReward = reward.totalReward;
        comp.parentObjective = Objective.ID;
        comp.CashMultiplier = Objective.CashMultiplier;
    }

    protected (bool completed, int bonuses, int maxBonuses, int totalReward) ResolveCompletion(int numCompletedTargets) =>
        (
            numCompletedTargets >= Objective.NumTargets.GetValueOrDefault(Difficulty, 0),
            Math.Min(numCompletedTargets - Objective.NumTargets.GetValueOrDefault(Difficulty, 0), Objective.BonusCap),
            Objective.BonusCap,
            numCompletedTargets >= Objective.NumTargets.GetValueOrDefault(Difficulty, 0) ?
                Objective.BaseReward.GetValueOrDefault(Difficulty, 0) + Objective.Bonus * Math.Min(numCompletedTargets - Objective.NumTargets.GetValueOrDefault(Difficulty, 0), Objective.BonusCap) :
                0
        );

    protected EntityUid? GetExpeditionConsole(EntityUid shuttle)
    {
        if (!EntMan.TryGetComponent<TransformComponent>(shuttle, out var shuttleTransform))
            return null;

        var children = shuttleTransform.ChildEnumerator;
        while (children.MoveNext(out var child))
            if (EntMan.TryGetComponent<SalvageExpeditionConsoleComponent>(child, out _))
                return child;

        return null;
    }

    protected HashSet<EntityUid> GetAllMarkedEntitiesOnShuttle(EntityUid shuttle) =>
        GetAllMarkedEntities()
        .Where(p => EntMan.TryGetComponent<TransformComponent>(p, out var transform) && transform.GridUid == shuttle)
        .ToHashSet();

    protected HashSet<EntityUid> GetAllMarkedEntities()
    {
        HashSet<EntityUid> res = [];

        var query = EntMan.AllEntityQueryEnumerator<SalvageMissionObjectiveTargetComponent>();
        while (query.MoveNext(out var uid, out var comp))
            if (comp.OwnedBy == Objective.ID)
                res.Add(uid);

        return res;
    }

    protected void DeleteWithEffect(HashSet<EntityUid> entities)
    {
        foreach (var uid in entities)
            DeleteWithEffect(uid);
    }

    protected void DeleteWithEffect(EntityUid uid)
    {
        var transform = EntMan.System<TransformSystem>();
        var audio = EntMan.System<AudioSystem>();
        
        if (transform.TryGetMapOrGridCoordinates(uid, out var pos))
        {
            var effect = EntMan.SpawnAtPosition(Objective.DeleteTargetEffect, pos.Value);
            audio.PlayPvs(Objective.DeleteTargetSound, effect);
        }

        EntMan.DeleteEntity(uid);
    }

    protected Entity<SalvageMissionObjectiveTargetComponent> SpawnAndMarkEntity(EntProtoId proto, EntityCoordinates pos)
    {
        var uid = EntMan.SpawnAtPosition(proto, pos);
        var comp = MarkEntity(uid);
        return (uid, comp);
    }

    protected SalvageMissionObjectiveTargetComponent MarkEntity(EntityUid uid)
    {
        var comp = EntMan.EnsureComponent<SalvageMissionObjectiveTargetComponent>(uid);
        comp.OwnedBy = Objective.ID;
        return comp;
    }
}