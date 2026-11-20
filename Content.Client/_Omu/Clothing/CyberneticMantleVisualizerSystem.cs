// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Omu.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Client._Omu.Clothing;

/// <summary>
/// Applies the wearer's synchronized eye color to the mantle's final clothing layer.
/// </summary>
public sealed class CyberneticMantleVisualizerSystem : VisualizerSystem<CyberneticMantleComponent>
{
    [Dependency] private readonly SharedItemSystem _items = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CyberneticMantleComponent, GetEquipmentVisualsEvent>(OnGetVisuals,
            after: [typeof(Content.Client.Clothing.ClientClothingSystem)]);
    }

    protected override void OnAppearanceChange(EntityUid uid, CyberneticMantleComponent component, ref AppearanceChangeEvent args)
    {
        _items.VisualsChanged(uid);
    }

    private void OnGetVisuals(Entity<CyberneticMantleComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (args.Slot == "mask" && args.Layers.Count > 0 &&
            AppearanceSystem.TryGetData<Color>(ent, CyberneticMantleVisuals.EyeColor, out var color))
            args.Layers[^1].Item2.Color = color;
    }
}
