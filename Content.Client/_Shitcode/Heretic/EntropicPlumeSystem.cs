// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class EntropicPlumeSystem : SharedEntropicPlumeSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((uid, sprite), EntropicPlumeKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((uid, sprite), layer);
    }

    private void OnStartup(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((uid, sprite), EntropicPlumeKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((uid, sprite), comp.Sprite);
        _sprite.LayerMapSet((uid, sprite), EntropicPlumeKey.Key, layer);
    }
}
