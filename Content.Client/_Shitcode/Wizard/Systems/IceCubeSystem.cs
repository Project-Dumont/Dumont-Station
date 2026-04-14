// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Traps;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Systems;

public sealed class IceCubeSystem : SharedIceCubeSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    protected override void Shutdown(Entity<IceCubeComponent> ent)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((uid, sprite), IceCubeKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((uid, sprite), layer);
    }

    protected override void Startup(Entity<IceCubeComponent> ent)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((uid, sprite), IceCubeKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((uid, sprite), comp.Sprite);
        _sprite.LayerMapSet((uid, sprite), IceCubeKey.Key, layer);
    }
}
