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

using Content.Shared.Trigger.Components.Effects;
using Content.Shared.Trigger.Systems;

namespace Content.Shared.Trigger;

/// <summary>
/// This is a base Trigger system which handles all the boilerplate for triggers automagically!
/// </summary>
public abstract partial class TriggerOnXSystem : EntitySystem
{
    [Dependency] protected TriggerSystem Trigger = default!;
}

/// <summary>
/// This is a base Trigger system which handles all the boilerplate for triggers automagically!
/// </summary>
public abstract partial class XOnTriggerSystem<T> : EntitySystem where T : BaseXOnTriggerComponent
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<T, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<T> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target is not { } uid)
            return;

        OnTrigger(ent, uid, ref args);
    }

    protected abstract void OnTrigger(Entity<T> ent, EntityUid target, ref TriggerEvent args);
}
