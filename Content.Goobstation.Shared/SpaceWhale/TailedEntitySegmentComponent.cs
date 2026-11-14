// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

using Robust.Shared.Map;
// Dumont end

namespace Content.Goobstation.Shared.SpaceWhale;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class TailedEntitySegmentComponent : Component
{
    [DataField, AutoNetworkedField]
    public NetCoordinates? Coords;

    [DataField, AutoNetworkedField]
    public Angle WorldRotation;

    [DataField, AutoNetworkedField]
    public int Order;

    [DataField, AutoNetworkedField]
    public int SegmentCount;

    [DataField, AutoNetworkedField]
    public EntityUid? Head;

    [DataField]
    public string? SegmentSpriteState;

    [DataField]
    public string? TailSpriteState;
}
