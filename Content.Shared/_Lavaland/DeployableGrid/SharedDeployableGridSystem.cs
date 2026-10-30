// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;

namespace Content.Shared._Lavaland.DeployableGrid;

public abstract class SharedDeployableGridSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeployableGridComponent, UseInHandEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, DeployableGridComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.DeployTime, new DeployableGridDoAfterEvent(), uid, used: uid)
        {
            BreakOnMove = true,
            NeedHand = true,
        };

        if (!CheckCanDeploy((uid, component), args.User))
        {
            args.Handled = true;
            return;
        }

        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
        args.Handled = true;
    }

    protected bool CheckCanDeploy(Entity<DeployableGridComponent> ent, EntityUid? user = null)
    {
        var check = GetDeploymentCheck(ent);
        if (check.FailureMessage is not {} message)
            return true;

        _popup.PopupPredictedCoordinates(Loc.GetString(message), Transform(ent).Coordinates, user);
        return false;
    }

    /// <summary>
    /// Evaluates the current map without side effects, including the exact shelter footprint.
    /// </summary>
    public DeployableGridCheck GetDeploymentCheck(Entity<DeployableGridComponent> ent)
    {
        var xform = Transform(ent);
        var comp = ent.Comp;
        var worldPos = _transform.GetMapCoordinates(ent, xform);
        var origin = (worldPos.Position + comp.Offset).Rounded();
        var box = new Box2(origin, origin + comp.BoxSize);

        // Works only on planets!
        if (xform.GridUid == null || xform.MapUid == null || xform.GridUid != xform.MapUid || !TryComp<MapGridComponent>(xform.GridUid.Value, out var gridComp))
        {
            return new DeployableGridCheck(box, false, Array.Empty<EntityUid>(), Array.Empty<EntityUid>());
        }

        var blockers = _mapSystem.GetAnchoredEntities(xform.GridUid.Value, gridComp, box)
            .Where(uid => !HasComp<ShelterClearableComponent>(uid)).ToArray();
        var nearby = _lookup.GetEntitiesInRange<MapGridComponent>(xform.Coordinates, comp.BoxSize.Length())
            .Select(grid => grid.Owner).ToArray();
        return new DeployableGridCheck(box, true, blockers, nearby);
    }
}
