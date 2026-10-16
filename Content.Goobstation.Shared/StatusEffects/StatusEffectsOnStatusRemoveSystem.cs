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
// Dumont end

using Content.Shared.StatusEffectNew;

namespace Content.Goobstation.Shared.StatusEffects;

public sealed partial class StatusEffectsOnStatusRemoveSystem : EntitySystem
{
    [Dependency] private StatusEffectsSystem _status = default!;

    private readonly Dictionary<EntityUid, Dictionary<EntProtoId, TimeSpan>> _toApply = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectsOnStatusRemoveComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnRemove(Entity<StatusEffectsOnStatusRemoveComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!_toApply.TryGetValue(args.Target, out var existing))
        {
            // Dumont start
            _toApply.Add(args.Target, new(ent.Comp.StatusEffects));
            // Dumont end
            return;
        }

        foreach (var (key, value) in ent.Comp.StatusEffects)
        {
            if (existing.TryGetValue(key, out var existingTime))
            {
                if (existingTime < value)
                    existing[key] = value;
            }
            else
                existing.Add(key, value);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_toApply.Count == 0)
            return;

        foreach (var (uid, dict) in _toApply)
        {
            foreach (var (key, value) in dict)
            {
                _status.TryUpdateStatusEffectDuration(uid, key, out _, value);
            }
        }

        _toApply.Clear();
    }
}
