// SPDX-FileCopyrightText: 2024 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyle Tyo <36606155+VerinSenpai@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Hands.Systems;
using Content.Shared.Atmos.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.RCD;
using Content.Shared.RCD.Components;
using Robust.Client.Placement;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;

namespace Content.Client.RCD;

/// <summary>
/// System for handling structure ghost placement in places where RCD can create objects.
/// </summary>
public sealed class RCDConstructionGhostSystem : EntitySystem
{
    private const string PlacementMode = nameof(AlignRCDConstruction);
    private const string RpdPlacementMode = nameof(AlignRPDAtmosPipeLayers);

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPlacementManager _placementManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    private Direction _placementDirection = default;
    private bool _useMirrorPrototype = false;
    public event EventHandler? FlipConstructionPrototype;

    public override void Initialize()
    {
        base.Initialize();

        // bind key
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.EditorFlipObject,
                new PointerInputCmdHandler(HandleFlip, outsidePrediction: true))
            .Register<RCDConstructionGhostSystem>();
    }

    public override void Shutdown()
    {
        CommandBinds.Unregister<RCDConstructionGhostSystem>();
        base.Shutdown();
    }

    private bool HandleFlip(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (args.State == BoundKeyState.Down)
        {
            if (!_placementManager.IsActive || _placementManager.Eraser)
                return false;

            var placerEntity = _placementManager.CurrentPermission?.MobUid;

            if (!TryComp<RCDComponent>(placerEntity, out var rcd))
                return false;

            var prototype = _protoManager.Index(rcd.ProtoId);
            if (string.IsNullOrEmpty(prototype.MirrorPrototype))
                return false;

            _useMirrorPrototype = !rcd.UseMirrorPrototype;

            var useProto = _useMirrorPrototype ? prototype.MirrorPrototype : prototype.Prototype;
            CreatePlacer(placerEntity.Value, rcd, useProto, prototype.Mode);

            // tell the server

            RaiseNetworkEvent(new RCDConstructionGhostFlipEvent(GetNetEntity(placerEntity.Value), _useMirrorPrototype));
        }

        return true;
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Get current placer data
        var placerEntity = _placementManager.CurrentPermission?.MobUid;
        var placerProto = _placementManager.CurrentPermission?.EntityType;
        var placerIsRCD = HasComp<RCDComponent>(placerEntity);

        // Exit if erasing or the current placer is not an RCD (build mode is active)
        if (_placementManager.Eraser || (placerEntity != null && !placerIsRCD))
            return;

        // Determine if player is carrying an RCD in their active hand
        if (_playerManager.LocalSession?.AttachedEntity is not { } player)
            return;

        var heldEntity = _hands.GetActiveItem(player);

        if (!TryComp<RCDComponent>(heldEntity, out var rcd))
        {
            // If the player was holding an RCD, but is no longer, cancel placement
            if (placerIsRCD)
                _placementManager.Clear();

            return;
        }
        var prototype = _protoManager.Index(rcd.ProtoId);

        // Update the direction the RCD prototype based on the placer direction
        if (_placementDirection != _placementManager.Direction)
        {
            _placementDirection = _placementManager.Direction;
            RaiseNetworkEvent(new RCDConstructionGhostRotationEvent(GetNetEntity(heldEntity.Value), _placementDirection));
        }
        // If the placer has not changed build it.
        var useProto = (_useMirrorPrototype && !string.IsNullOrEmpty(prototype.MirrorPrototype)) ? prototype.MirrorPrototype : prototype.Prototype;

        // Funky - Check if RPD and prototype supports layered placement
        if (rcd.IsRpd && useProto != null && !prototype.NoLayers)
        {
            _placementManager.Clear();
            CreateLayeredPlacer(heldEntity.Value, rcd, useProto, prototype.Mode);
        }
        else if (heldEntity != placerEntity || useProto != placerProto)
        {
            _placementManager.Clear();
            CreatePlacer(heldEntity.Value, rcd, useProto, prototype.Mode);
        }


    }

    private void CreatePlacer(EntityUid uid, RCDComponent component, string? prototype, RcdMode mode)
    {
        // Create a new placer
        var newObjInfo = new PlacementInformation
        {
            MobUid = uid,
            PlacementOption = PlacementMode,
            EntityType = prototype,
            Range = (int) Math.Ceiling(SharedInteractionSystem.InteractionRange),
            IsTile = mode == RcdMode.ConstructTile,
            UseEditorContext = false,
        };

        _placementManager.Clear();
        _placementManager.BeginPlacing(newObjInfo);
    }

    private void CreateLayeredPlacer(EntityUid uid, RCDComponent component, string? prototype, RcdMode mode)
    {
        // Create a layer placer
        var newObjInfo = new PlacementInformation
        {
            MobUid = uid,
            PlacementOption = RpdPlacementMode,
            EntityType = prototype,
            Range = (int) Math.Ceiling(SharedInteractionSystem.InteractionRange),
            IsTile = mode == RcdMode.ConstructTile,
            UseEditorContext = false,
        };

        _placementManager.BeginPlacing(newObjInfo);
    }
}
