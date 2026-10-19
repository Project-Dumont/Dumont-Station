// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Shared.Antag;
using Content.Shared.Ghost;
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
    private const string LeaderAuraState = "leader_aura";
    private const string LeaderHaloState = "halo"; // Dumont

    private static readonly Vector2 LeaderHaloOffset = new(0f, 0.2f); // Dumont

    private readonly Dictionary<EntityUid, List<int>> _hiddenLayers = new();

    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IPlayerManager _player = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PentagramComponent, ComponentStartup>(OnPentagramAdded);
        SubscribeLocalEvent<PentagramComponent, ComponentShutdown>(OnPentagramRemoved);
        SubscribeLocalEvent<BloodCultLeaderComponent, ComponentStartup>(OnLeaderAdded);
        SubscribeLocalEvent<BloodCultLeaderComponent, ComponentShutdown>(OnLeaderRemoved);

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

    private void OnPentagramAdded(EntityUid uid, PentagramComponent component, ComponentStartup args) =>
        RefreshPentagramVisual(uid, component); // Dumont

    // Dumont
    private void RefreshPentagramVisual(EntityUid uid, PentagramComponent component)
    {
        if (HasComp<BloodCultLeaderComponent>(uid))
        {
            RemovePentagram(uid);
            AddLeaderGlow(uid);
            return;
        }

        RemoveLeaderGlow(uid);
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

    private void OnPentagramRemoved(EntityUid uid, PentagramComponent component, ComponentShutdown args)
    {
        RemovePentagram(uid);
        RemoveLeaderGlow(uid); // Dumont
    }

    private void OnLeaderAdded(EntityUid uid, BloodCultLeaderComponent component, ComponentStartup args)
    {
        if (TryComp<PentagramComponent>(uid, out var pentagram))
            RefreshPentagramVisual(uid, pentagram); // Dumont
    }

    private void OnLeaderRemoved(EntityUid uid, BloodCultLeaderComponent component, ComponentShutdown args)
    {
        RemoveLeaderGlow(uid); // Dumont

        if (!TerminatingOrDeleted(uid) && TryComp<PentagramComponent>(uid, out var pentagram))
            AddPentagram(uid, pentagram); // Dumont
    }

    private void RemovePentagram(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) ||
            !sprite.LayerMapTryGet(PentagramKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    // Dumont changes start
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BloodCultLeaderComponent, SpriteComponent>();

        while (query.MoveNext(out _, out var sprite))
        {
            if (!sprite.LayerMapTryGet(BloodCultVisualLayers.LeaderBody, out var body))
                continue;

            if (!sprite.LayerMapTryGet(BloodCultVisualLayers.LeaderAura, out var aura))
                aura = -1;

            if (!sprite.LayerMapTryGet(BloodCultVisualLayers.LeaderHalo, out var halo))
                halo = -1;

            for (var i = 0; i < sprite.AllLayers.Count(); i++)
            {
                if (i == body || i == aura || i == halo)
                    continue;

                if (sprite.TryGetLayer(i, out var layer) && layer.Visible)
                    sprite.LayerSetVisible(i, false);
            }
        }
    }

    private void AddLeaderGlow(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) ||
            sprite.LayerMapTryGet(BloodCultVisualLayers.LeaderBody, out _))
            return;

        var restore = new List<int>();

        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            if (sprite.TryGetLayer(i, out var existing) && existing.Visible)
                restore.Add(i);

            sprite.LayerSetVisible(i, false);
        }

        _hiddenLayers[uid] = restore;

        var body = sprite.AddLayer(new SpriteSpecifier.Rsi(LeaderBodyRsi, LeaderBodyState));
        sprite.LayerMapSet(BloodCultVisualLayers.LeaderBody, body);

        var aura = sprite.AddLayer(new SpriteSpecifier.Rsi(LeaderAuraRsi, LeaderAuraState));
        sprite.LayerMapSet(BloodCultVisualLayers.LeaderAura, aura);

        var halo = sprite.AddLayer(new SpriteSpecifier.Rsi(LeaderHaloRsi, LeaderHaloState));
        sprite.LayerMapSet(BloodCultVisualLayers.LeaderHalo, halo);
        sprite.LayerSetOffset(halo, LeaderHaloOffset);
    }

    private void RemoveLeaderGlow(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
        {
            _hiddenLayers.Remove(uid);
            return;
        }

        if (sprite.LayerMapTryGet(BloodCultVisualLayers.LeaderHalo, out var halo))
            sprite.RemoveLayer(halo);

        if (sprite.LayerMapTryGet(BloodCultVisualLayers.LeaderAura, out var aura))
            sprite.RemoveLayer(aura);

        if (sprite.LayerMapTryGet(BloodCultVisualLayers.LeaderBody, out var body))
            sprite.RemoveLayer(body);

        if (!_hiddenLayers.Remove(uid, out var restore))
            return;

        foreach (var index in restore)
            sprite.LayerSetVisible(index, true);
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
