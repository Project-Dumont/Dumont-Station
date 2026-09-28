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

using Content.Shared.Actions;
// Dumont end

namespace Content.Goobstation.Shared.SpaceWhale;

[ByRefEvent]
public record struct GetTailedEntitySegmentCountEvent(int Amount);

[ByRefEvent]
public readonly record struct UpdateTailedEntitySegmentCountEvent(int Amount);

public sealed partial class TailedEntityForceContractEvent : InstantActionEvent;
