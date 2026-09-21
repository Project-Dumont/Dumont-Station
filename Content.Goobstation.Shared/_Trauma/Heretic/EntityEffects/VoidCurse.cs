// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Systems;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.Heretic.Systems.PathSpecific.Void;
// Dumont end

namespace Content.Trauma.Shared.Heretic.EntityEffects;

public sealed partial class VoidCurse : EntityEffectBase<VoidCurse>
{
    [DataField]
    public int Stacks = 1;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-void-curse");
}

public sealed partial class VoidCurseEffectSystem : EntityEffectSystem<TransformComponent, VoidCurse>
{
    [Dependency] private SharedVoidCurseSystem _voidCurse = default!;

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<VoidCurse> args)
    {
        _voidCurse.DoCurse(ent, args.Effect.Stacks);
    }
}
