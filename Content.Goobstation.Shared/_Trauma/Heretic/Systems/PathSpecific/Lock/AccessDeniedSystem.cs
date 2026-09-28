// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Trauma.Common.Heretic;
using Content.Trauma.Shared.Heretic.Components.StatusEffects;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems.PathSpecific.Lock;

public sealed partial class AccessDeniedSystem : EntitySystem
{
    [Dependency] private StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectContainerComponent, BeforeAccessReaderCheckEvent>(OnBeforeCheck);
    }

    private void OnBeforeCheck(Entity<StatusEffectContainerComponent> ent, ref BeforeAccessReaderCheckEvent args)
    {
        if (_status.HasEffectComp<AccessDeniedStatusEffectComponent>(ent))
            args.Cancelled = true;
    }
}
