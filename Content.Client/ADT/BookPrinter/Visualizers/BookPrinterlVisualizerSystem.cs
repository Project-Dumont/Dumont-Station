// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Оригинал данного файла был сделан @temporaldarkness (discord). Код был взят с https://github.com/ss14-ganimed/ENT14-Master.

using Content.Shared.ADT.BookPrinter.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Client.GameObjects;

namespace Content.Client.ADT.BookPrinter.Visualizers;

public sealed partial class BookPrinterVisualizerSystem : VisualizerSystem<BookPrinterVisualsComponent>
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    protected override void OnAppearanceChange(EntityUid uid, BookPrinterVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
            || !TryComp<ItemSlotsComponent>(uid, out var slotComp))
            return;

        var ent = (uid, args.Sprite);

        if (SpriteSystem.LayerMapTryGet(ent, BookPrinterVisualLayers.Working, out var workLayer, false))
        {
            SpriteSystem.LayerSetVisible(ent, workLayer, component.DoWorkAnimation);
        }

        var cartridgeUid = _itemSlotsSystem.GetItemOrNull(uid, "cartridgeSlot", slotComp);

        if (SpriteSystem.LayerMapTryGet(ent, BookPrinterVisualLayers.Slotted, out var slotLayer, false))
        {
            var value = cartridgeUid is not null;
            SpriteSystem.LayerSetVisible(ent, slotLayer, value);
        }

        if (SpriteSystem.LayerMapTryGet(ent, BookPrinterVisualLayers.Full, out var fullLayer, false))
        {
            SpriteSystem.LayerSetVisible(ent, fullLayer, false);

            if (cartridgeUid is not null
                && TryComp<BookPrinterCartridgeComponent>(cartridgeUid, out var cartridgeComp))
            {
                var isVisible = cartridgeComp.CurrentCharge == cartridgeComp.FullCharge;
                SpriteSystem.LayerSetVisible(ent, fullLayer, isVisible);
            }
        }

        if (SpriteSystem.LayerMapTryGet(ent, BookPrinterVisualLayers.High, out var highLayer, false))
        {
            SpriteSystem.LayerSetVisible(ent, highLayer, false);

            if (cartridgeUid is not null
                && TryComp<BookPrinterCartridgeComponent>(cartridgeUid, out var cartridgeComp))
            {
                var isVisible = cartridgeComp.CurrentCharge >= cartridgeComp.FullCharge * 0.7f && cartridgeComp.CurrentCharge < cartridgeComp.FullCharge;
                SpriteSystem.LayerSetVisible(ent, highLayer, isVisible);
            }
        }

        if (SpriteSystem.LayerMapTryGet(ent, BookPrinterVisualLayers.Medium, out var mediumLayer, false))
        {
            SpriteSystem.LayerSetVisible(ent, mediumLayer, false);

            if (cartridgeUid is not null
                && TryComp<BookPrinterCartridgeComponent>(cartridgeUid, out var cartridgeComp))
            {
                var isVisible = cartridgeComp.CurrentCharge >= cartridgeComp.FullCharge * 0.4f && cartridgeComp.CurrentCharge < cartridgeComp.FullCharge / 1.43f;
                SpriteSystem.LayerSetVisible(ent, mediumLayer, isVisible);
            }
        }

        if (SpriteSystem.LayerMapTryGet(ent, BookPrinterVisualLayers.Low, out var lowLayer, false))
        {
            SpriteSystem.LayerSetVisible(ent, lowLayer, false);

            if (cartridgeUid is not null
                && TryComp<BookPrinterCartridgeComponent>(cartridgeUid, out var cartridgeComp))
            {
                var isVisible = cartridgeComp.CurrentCharge > 0 && cartridgeComp.CurrentCharge < cartridgeComp.FullCharge * 0.4f;
                SpriteSystem.LayerSetVisible(ent, lowLayer, isVisible);
            }
        }

        if (SpriteSystem.LayerMapTryGet(ent, BookPrinterVisualLayers.None, out var noneLayer, false))
        {
            SpriteSystem.LayerSetVisible(ent, noneLayer, false);

            if (cartridgeUid is not null
                && TryComp<BookPrinterCartridgeComponent>(cartridgeUid, out var cartridgeComp))
            {
                var isVisible = cartridgeComp.CurrentCharge < 1;
                SpriteSystem.LayerSetVisible(ent, noneLayer, isVisible);
            }
        }
    }
}
