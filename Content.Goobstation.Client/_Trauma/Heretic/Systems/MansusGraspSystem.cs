// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Systems;
// Dumont end

// Dumont start
using Robust.Client.Graphics;
// Dumont end

namespace Content.Trauma.Client.Heretic.Systems;

public sealed partial class MansusGraspSystem : SharedMansusGraspSystem
{
    [Dependency] private IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new AreaMansusGraspOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<AreaMansusGraspOverlay>();
    }
}
