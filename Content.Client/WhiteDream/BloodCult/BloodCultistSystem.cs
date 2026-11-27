// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Antag;
using Content.Shared.Ghost;
using Content.Shared.Hands.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusIcon.Components;
using Content.Shared.WhiteDream.BloodCult;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Components;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Client.WhiteDream.BloodCult;

public sealed partial class BloodCultistSystem : EntitySystem
{
    private static readonly ResPath LeaderBodyRsi =
        new("/Textures/_Dumont/BloodCult/Mobs/bloodcultleech.rsi");

    private static readonly ResPath LeaderAuraRsi =
        new("/Textures/WhiteDream/BloodCult/Effects/leader_aura.rsi");

    private static readonly ResPath LeaderHaloRsi =
        new("/Textures/_Dumont/BloodCult/Effects/halo.rsi"); // Dumont

    private const string LeaderBodyState = "leech";
    private const string LeaderCritState = "crit"; // Dumont
    private const string LeaderDeadState = "dead"; // Dumont
    private const string LeaderAuraState = "leader_aura";
    private const string LeaderHaloState = "halo"; // Dumont

    private static readonly Vector2 LeaderHaloOffset = new(0f, 0.2f); // Dumont

    // Dumont changes start
    // the layer map takes object keys, so box these once instead of every frame
    private static readonly object LeaderBodyKey = BloodCultVisualLayers.LeaderBody;
    private static readonly object LeaderAuraKey = BloodCultVisualLayers.LeaderAura;
    private static readonly object LeaderHaloKey = BloodCultVisualLayers.LeaderHalo;

    private readonly Dictionary<EntityUid, HashSet<ISpriteLayer>> _hiddenLayers = new();
    private readonly List<int> _keptLayers = new();
    // Dumont end

    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IPlayerManager _player = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PentagramComponent, ComponentStartup>(OnPentagramAdded);
        SubscribeLocalEvent<PentagramComponent, ComponentShutdown>(OnPentagramRemoved);

        // Dumont changes start
        SubscribeLocalEvent<BloodCultLeaderFormComponent, ComponentStartup>(OnLeaderFormAdded);
        SubscribeLocalEvent<BloodCultLeaderFormComponent, ComponentShutdown>(OnLeaderFormRemoved);
        SubscribeLocalEvent<BloodCultLeaderFormComponent, MobStateChangedEvent>(OnLeaderFormMobState);
        // Dumont end

        SubscribeLocalEvent<ConstructComponent, GetStatusIconsEvent>(GetCultStatusIcon);
        SubscribeLocalEvent<BloodCultistComponent, GetStatusIconsEvent>(GetBloodCultistStatusIcon);
        SubscribeLocalEvent<BloodCultLeaderComponent, GetStatusIconsEvent>(GetCultStatusIcon);
        SubscribeLocalEvent<BloodCultMarkComponent, GetStatusIconsEvent>(GetCultStatusIcon); // Dumont

        SubscribeLocalEvent<ConstructComponent, CanDisplayStatusIconsEvent>(OnCanShowCultIcon);
        SubscribeLocalEvent<BloodCultistComponent, CanDisplayStatusIconsEvent>(OnCanShowCultIcon);
        SubscribeLocalEvent<BloodCultLeaderComponent, CanDisplayStatusIconsEvent>(OnCanShowCultIcon);
        SubscribeLocalEvent<BloodCultMarkComponent, CanDisplayStatusIconsEvent>(OnCanShowCultIcon); // Dumont
    }

    private void GetCultStatusIcon<T>(Entity<T> ent, ref GetStatusIconsEvent args)
        where T : IComponent, IAntagStatusIconComponent
    {
        var canEv = new CanDisplayStatusIconsEvent(_player.LocalSession?.AttachedEntity);
        RaiseLocalEvent(ent, ref canEv);

        if (canEv.Cancelled || !_prototype.TryIndex(ent.Comp.StatusIcon, out var icon))
            return;

        args.StatusIcons.Add(icon);
    }

    private void GetBloodCultistStatusIcon(Entity<BloodCultistComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<BloodCultLeaderComponent>(ent))
            return;

        GetCultStatusIcon(ent, ref args);
    }

    private void OnPentagramAdded(EntityUid uid, PentagramComponent component, ComponentStartup args)
    {
        if (HasComp<BloodCultLeaderFormComponent>(uid)) // Dumont
            return;

        AddPentagram(uid, component);
    }

    private void AddPentagram(EntityUid uid, PentagramComponent component)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(PentagramKey.Key, out _))
        {
            var randomState = _random.Pick(component.States);

            var layer = sprite.AddLayer(new SpriteSpecifier.Rsi(component.RsiPath, randomState));

            sprite.LayerMapSet(PentagramKey.Key, layer);
            sprite.LayerSetOffset(layer, component.Offset); // Dumont
        }
    }

    private void OnPentagramRemoved(EntityUid uid, PentagramComponent component, ComponentShutdown args) =>
        RemovePentagram(uid);

    private void RemovePentagram(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) ||
            !sprite.LayerMapTryGet(PentagramKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    // Dumont changes start
    private void OnLeaderFormAdded(Entity<BloodCultLeaderFormComponent> ent, ref ComponentStartup args)
    {
        RemovePentagram(ent);
        AddLeaderGlow(ent);
    }

    private void OnLeaderFormRemoved(Entity<BloodCultLeaderFormComponent> ent, ref ComponentShutdown args)
    {
        RemoveLeaderGlow(ent);

        if (!TerminatingOrDeleted(ent) && TryComp<PentagramComponent>(ent, out var pentagram))
            AddPentagram(ent, pentagram);
    }

    private void OnLeaderFormMobState(Entity<BloodCultLeaderFormComponent> ent, ref MobStateChangedEvent args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            UpdateLeaderBody(sprite, args.NewMobState);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BloodCultLeaderFormComponent, SpriteComponent>();

        while (query.MoveNext(out var uid, out _, out var sprite))
            HideCoveredLayers(uid, sprite);
    }

    private void AddLeaderGlow(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || sprite.LayerMapTryGet(LeaderBodyKey, out _))
            return;

        _hiddenLayers[uid] = new HashSet<ISpriteLayer>();

        var body = sprite.AddLayer(new SpriteSpecifier.Rsi(LeaderBodyRsi, LeaderBodyState));
        sprite.LayerMapSet(LeaderBodyKey, body);

        var aura = sprite.AddLayer(new SpriteSpecifier.Rsi(LeaderAuraRsi, LeaderAuraState));
        sprite.LayerMapSet(LeaderAuraKey, aura);

        var halo = sprite.AddLayer(new SpriteSpecifier.Rsi(LeaderHaloRsi, LeaderHaloState));
        sprite.LayerMapSet(LeaderHaloKey, halo);
        sprite.LayerSetOffset(halo, LeaderHaloOffset);

        UpdateLeaderBody(sprite, CompOrNull<MobStateComponent>(uid)?.CurrentState ?? MobState.Alive);
        HideCoveredLayers(uid, sprite);
    }

    private void RemoveLeaderGlow(EntityUid uid)
    {
        _hiddenLayers.Remove(uid, out var hidden);

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        // top down, removing a layer shifts everything above it
        RemoveLeaderLayer(sprite, LeaderHaloKey);
        RemoveLeaderLayer(sprite, LeaderAuraKey);
        RemoveLeaderLayer(sprite, LeaderBodyKey);

        if (hidden is null)
            return;

        // only walk what the sprite still has, a removed layer throws when you touch it
        foreach (var layer in sprite.AllLayers)
        {
            if (hidden.Contains(layer))
                layer.Visible = true;
        }
    }

    /// <summary>
    ///     The husk stays on the corpse, so the aura and the halo go once the leader is dead.
    /// </summary>
    private void UpdateLeaderBody(SpriteComponent sprite, MobState state)
    {
        if (!sprite.LayerMapTryGet(LeaderBodyKey, out var body))
            return;

        sprite.LayerSetState(body,
            state switch
            {
                MobState.Dead => LeaderDeadState,
                MobState.SoftCritical or MobState.HardCritical => LeaderCritState,
                _ => LeaderBodyState,
            });

        if (state != MobState.Dead)
            return;

        RemoveLeaderLayer(sprite, LeaderHaloKey);
        RemoveLeaderLayer(sprite, LeaderAuraKey);
    }

    /// <summary>
    ///     Nothing stops inventory or humanoid code from adding and revealing layers while the husk is on, so sweep
    ///     them every frame. Hands are left alone, whatever the leader is holding stays visible.
    /// </summary>
    private void HideCoveredLayers(EntityUid uid, SpriteComponent sprite)
    {
        if (!_hiddenLayers.TryGetValue(uid, out var hidden))
            return;

        _keptLayers.Clear();
        CollectKeptLayers(uid, sprite, _keptLayers);

        var index = 0;

        foreach (var layer in sprite.AllLayers)
        {
            if (layer.Visible && !_keptLayers.Contains(index))
            {
                layer.Visible = false;
                hidden.Add(layer);
            }

            index++;
        }
    }

    private void CollectKeptLayers(EntityUid uid, SpriteComponent sprite, List<int> kept)
    {
        if (sprite.LayerMapTryGet(LeaderBodyKey, out var body))
            kept.Add(body);

        if (sprite.LayerMapTryGet(LeaderAuraKey, out var aura))
            kept.Add(aura);

        if (sprite.LayerMapTryGet(LeaderHaloKey, out var halo))
            kept.Add(halo);

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        foreach (var revealed in hands.RevealedLayers.Values)
        {
            foreach (var key in revealed)
            {
                if (sprite.LayerMapTryGet(key, out var layer))
                    kept.Add(layer);
            }
        }
    }

    private static void RemoveLeaderLayer(SpriteComponent sprite, object key)
    {
        if (sprite.LayerMapTryGet(key, out var layer))
            sprite.RemoveLayer(layer);
    }
    // Dumont end

    /// <summary>
    /// Determine whether a client should display the cult icon.
    /// </summary>
    private void OnCanShowCultIcon<T>(EntityUid uid, T comp, ref CanDisplayStatusIconsEvent args)
        where T : IAntagStatusIconComponent
    {
        if (!CanDisplayIcon(args.User, comp.IconVisibleToGhost))
            args.Cancelled = true;
    }

    /// <summary>
    /// The criteria that determine whether a client should see Cult/Cult leader icons.
    /// </summary>
    private bool CanDisplayIcon(EntityUid? uid, bool visibleToGhost)
    {
        if (HasComp<BloodCultistComponent>(uid) || HasComp<BloodCultLeaderComponent>(uid) ||
            HasComp<ConstructComponent>(uid))
            return true;

        return visibleToGhost && HasComp<GhostComponent>(uid);
    }
}

internal enum BloodCultVisualLayers : byte
{
    LeaderBody, // Dumont
    LeaderAura,
    LeaderHalo // Dumont
}
