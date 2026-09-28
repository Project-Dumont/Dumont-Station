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
// Dumont end

namespace Content.Server.Store.Conditions;

public sealed partial class BuyBeforeCondition
{
    /// <summary>
    /// If false, only one of the listings needs to be purchased to pass the whitelist.
    /// If true, all of the listings need to be purchased.
    /// </summary>
    [DataField]
    public bool WhitelistRequireAll = true;
}
