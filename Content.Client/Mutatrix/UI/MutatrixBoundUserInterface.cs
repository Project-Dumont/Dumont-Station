// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mutatrix.Events;
using Content.Shared.Mutatrix.Prototypes;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Mutatrix.UI;

/// <summary>
/// Client BUI for the Mutatrix radial transformation menu.
/// </summary>
[UsedImplicitly]
public sealed class MutatrixBoundUserInterface : BoundUserInterface
{
    private static readonly HashSet<string> HiddenStaticTransformations = new()
    {
        "MutatrixRat",
        "MutatrixIPC",
        "MutatrixPlasmaman",
        "MutatrixRevenant",
        "MutatrixChitinid",
        "MutatrixFeroxi",
        "MutatrixArachnid",
        "MutatrixBaseMobAsteroid",
        "MutatrixBesta",
        "MutatrixChama",
        "MutatrixQuatroBracos",
        "MutatrixGreyMatter",
        "MutatrixGhoulStalker",
        "MutatrixGosma",
    };

    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private SimpleRadialMenu? _menu;

    public MutatrixBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();

        // Do not Track(Owner). During polymorph the eye moves to the new body
        // while the item/original body can be on another map.
        if (State is MutatrixBoundUserInterfaceState state)
            Refresh(state);
        else
            Refresh(new MutatrixBoundUserInterfaceState());

        _menu.Open();
        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not MutatrixBoundUserInterfaceState mutatrixState)
            return;

        Refresh(mutatrixState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _menu = null;
    }

    private void Refresh(MutatrixBoundUserInterfaceState state)
    {
        if (_menu == null)
            return;

        var options = new List<RadialMenuOption>();

        foreach (var proto in _prototype
                     .EnumeratePrototypes<MutatrixTransformationPrototype>()
                     .OrderBy(p => p.Order)
                     .ThenBy(p => p.ID))
        {
            if (HiddenStaticTransformations.Contains(proto.ID))
                continue;

            options.Add(BuildStaticOption(proto, state));
        }

        foreach (var prototypeId in state.DynamicUnlocked.OrderBy(id => id))
        {
            var option = BuildDynamicOption(prototypeId, state);
            if (option != null)
                options.Add(option);
        }

        _menu.SetButtons(options.ToArray(), new SimpleRadialMenuSettings
        {
            UseSectors = true,
            DisplayBorders = true,
            NoBackground = false,
            DefaultContainerRadius = 110,
        });
    }

    private RadialMenuOption BuildStaticOption(
        MutatrixTransformationPrototype proto,
        MutatrixBoundUserInterfaceState state)
    {
        var unlocked = state.Unlocked.Contains(proto.ID);
        var selected = state.Selected != null && state.Selected.Value == proto.ID && state.SelectedDynamic == null;

        var tooltip = Loc.GetString(proto.Name);
        if (!string.IsNullOrWhiteSpace(proto.Description))
            tooltip += "\n" + Loc.GetString(proto.Description);

        tooltip += "\n" + Loc.GetString(unlocked
            ? selected ? "mutatrix-menu-selected" : "mutatrix-menu-unlocked"
            : "mutatrix-menu-locked");

        var icon = proto.Icon ?? new SpriteSpecifier.EntityPrototype(proto.MobPrototype);

        if (!unlocked)
        {
            return new RadialMenuActionOption<ProtoId<MutatrixTransformationPrototype>>(_ => { }, proto.ID)
            {
                Sprite = icon,
                ToolTip = tooltip,
            };
        }

        return new RadialMenuActionOption<ProtoId<MutatrixTransformationPrototype>>(SelectStaticTransformation, proto.ID)
        {
            Sprite = icon,
            ToolTip = tooltip,
        };
    }

    private RadialMenuOption? BuildDynamicOption(string prototypeId, MutatrixBoundUserInterfaceState state)
    {
        var entProtoId = new EntProtoId(prototypeId);
        if (!_prototype.TryIndex(entProtoId, out var entityPrototype))
            return null;

        var selected = state.SelectedDynamic == prototypeId;
        var name = GetScannedDisplayName(prototypeId, entityPrototype);

        var tooltip = Loc.GetString("mutatrix-menu-scanned-dynamic", ("name", name));
        tooltip += "\n" + Loc.GetString(selected ? "mutatrix-menu-selected" : "mutatrix-menu-unlocked");

        return new RadialMenuActionOption<string>(SelectDynamicTransformation, prototypeId)
        {
            Sprite = new SpriteSpecifier.EntityPrototype(entProtoId),
            ToolTip = tooltip,
        };
    }

    private string GetScannedDisplayName(string prototypeId, EntityPrototype entityPrototype)
    {
        foreach (var species in _prototype.EnumeratePrototypes<SpeciesPrototype>())
        {
            if (species.Prototype.Id == prototypeId)
                return Loc.GetString(species.Name);
        }

        return string.IsNullOrWhiteSpace(entityPrototype.Name)
            ? prototypeId
            : Loc.GetString(entityPrototype.Name);
    }

    private void SelectStaticTransformation(ProtoId<MutatrixTransformationPrototype> transformation)
    {
        SendMessage(new MutatrixSelectTransformationMessage(transformation));
    }

    private void SelectDynamicTransformation(string entityPrototype)
    {
        SendMessage(new MutatrixSelectScannedPrototypeMessage(entityPrototype));
    }
}
