// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;

namespace Content.Trauma.Server.JobListings;

public sealed partial class BattlecruiserArrivalSystem : EntitySystem
{
    [Dependency] private ChatSystem _chat = default!;

    [SubscribeLocalEvent(after: [typeof(RuleGridsSystem)])]
    private void OnLoaded(Entity<BattlecruiserArrivalComponent> ent, ref RuleLoadedGridsEvent args)
    {
        _chat.DispatchGlobalAnnouncement(Loc.GetString("station-event-syndicate-battle-cruiser"),
            playSound: false, colorOverride: Color.Red);
    }
}
