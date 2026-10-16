// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
// Dumont start
using Content.Shared.Damage;
// Dumont end

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Server.Polymorph.Components;
using Content.Shared.Actions.Components;
using Content.Shared.Chat;
using Content.Shared.Damage.Components;
using Content.Trauma.Server.Heretic.Systems;
using Content.Trauma.Shared.Heretic.Components;
using Content.Trauma.Shared.Heretic.Components.Ghoul;
using Content.Trauma.Shared.Heretic.Events;
using Content.Trauma.Shared.Heretic.Rituals;
using Robust.Shared.Player;
using Robust.Shared.Timing;
// Dumont end

namespace Content.Trauma.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    [SubscribeLocalEvent]
    private void OnDraftsModify(Entity<HereticComponent> ent, ref HereticModifySideKnowledgeDraftsEvent args)
    {
        foreach (var (key, value) in args.SideKnowledgeDrafts)
        {
            if (ent.Comp.SideKnowledgeDrafts.TryGetValue(key, out var existing))
                ent.Comp.SideKnowledgeDrafts[key] = Math.Max(0, existing + value);
            else
                ent.Comp.SideKnowledgeDrafts.Add(key, Math.Max(0, value));
        }
    }
}
