// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Crucible.Systems;
using Robust.Client.Player;
// Dumont end

// Dumont start
using Robust.Client.Graphics;
// Dumont end

namespace Content.Trauma.Client.Heretic.Systems;

public sealed partial class XRayVisionSystem : SharedXRayVisionSystem
{
    [Dependency] private ILightManager _light = default!;
    [Dependency] private IPlayerManager _player = default!;

    protected override void DrawLight(EntityUid uid, bool value)
    {
        base.DrawLight(uid, value);

        if (_player.LocalEntity != uid)
            return;

        _light.DrawLighting = value;
    }
}
