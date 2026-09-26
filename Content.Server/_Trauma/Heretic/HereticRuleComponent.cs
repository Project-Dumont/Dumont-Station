// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Store;
// Dumont end

namespace Content.Trauma.Server.Heretic.Components;

[RegisterComponent]
public sealed partial class HereticRuleComponent : Component
{
    [DataField]
    public int RealityShiftPerHeretic = 1;

    [DataField]
    public bool HasAHereticAscended;

    [DataField]
    // Dumont start
    public EntProtoId? ERTEvent;
    // Dumont end

    public readonly List<EntityUid> Minds = new();

    public static readonly List<ProtoId<StoreCategoryPrototype>> StoreCategories = new()
    {
        "HereticPathAsh",
        "HereticPathLock",
        "HereticPathFlesh",
        "HereticPathBlade",
        "HereticPathVoid",
        "HereticPathRust",
        "HereticPathCosmos",
        "HereticPathSpecial",
        "HereticPathSideT1",
        "HereticPathSideT2",
        "HereticPathSideT3",
    };
}
