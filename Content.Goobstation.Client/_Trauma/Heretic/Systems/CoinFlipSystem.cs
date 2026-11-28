// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components.Side;
using Content.Trauma.Shared.Heretic.Systems.Side;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

namespace Content.Trauma.Client.Heretic.Systems;

public sealed partial class CoinFlipSystem : SharedCoinFlipSystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoinFlipComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<CoinFlipComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not { } sprite ||
            !Appearance.TryGetData(ent, CoinFlipVisuals.SpriteState, out string state, args.Component))
            return;

        _sprite.LayerSetRsiState((ent, sprite), CoinFlipKey.Key, state);
    }
}
