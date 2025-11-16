// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Wraith.SaltLines;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Wraith.SaltLines;

public sealed class SaltLineVisualizerSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SaltLineVisualizerComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<SaltLineVisualizerComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_appearanceSystem.TryGetData<SaltLineVisDirFlags>(ent.Owner, SaltLineVisuals.ConnectedMask, out var mask, args.Component))
            mask = SaltLineVisDirFlags.None;

        _spriteSystem.LayerSetRsiState((ent.Owner, args.Sprite), 0, $"{ent.Comp.State}{(int)mask}");
    }
}
