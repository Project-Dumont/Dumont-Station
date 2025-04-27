// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Dyable;

namespace Content.Shared.Crayon;

[Virtual]
public abstract class SharedCrayonSystem : EntitySystem
{
    protected static void OnDyeGetColor(EntityUid uid, SharedCrayonComponent component, GetDyableColorsEvent args)
    {
        args.Color = component.Color;
        args.Handled = true;
    }
}
