// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class ShadowCloakSystem : SharedShadowCloakSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    protected override void Startup(Entity<ShadowCloakedComponent> ent)
    {
        base.Startup(ent);

        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        ent.Comp.WasVisible = sprite.Visible;
        _sprite.SetVisible((ent, sprite), false);
    }

    protected override void Shutdown(Entity<ShadowCloakedComponent> ent)
    {
        base.Shutdown(ent);

        if (TryComp(ent, out SpriteComponent? sprite))
            _sprite.SetVisible((ent, sprite), ent.Comp.WasVisible);
    }
}
