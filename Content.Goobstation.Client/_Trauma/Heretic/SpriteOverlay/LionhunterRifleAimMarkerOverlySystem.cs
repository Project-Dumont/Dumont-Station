// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components.Side;
// Dumont end

namespace Content.Trauma.Client.Heretic.SpriteOverlay;

public sealed class LionhunterRifleAimMarkerOverlySystem : SpriteOverlaySystem<AimedRifleMarkerComponent>;
