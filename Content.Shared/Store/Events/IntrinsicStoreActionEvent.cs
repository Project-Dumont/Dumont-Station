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

namespace Content.Shared.Store.Events;

/// <summary>
/// Opens a store specified by <see cref="StoreComponent"/>
/// Used for entities with a store built into themselves like Revenant or PAI
/// </summary>
public sealed partial class IntrinsicStoreActionEvent : InstantActionEvent
{
}
