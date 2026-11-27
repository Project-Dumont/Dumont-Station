// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Revolutionary.Components;
using Content.Server.Roles;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Trauma.Shared.Revolutionary;
using Robust.Shared.Player;

namespace Content.Trauma.Server.Revolutionary;

public sealed class RevConversionSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly RevolutionaryRuleSystem _rev = default!;
    // Dumont changes start
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    // Dumont end

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevConvertedEvent>(OnRevConverted);
    }

    private void OnRevConverted(ref RevConvertedEvent args)
    {
        // Dumont changes start
        if (_mind.TryGetMind(args.User, out var userMindId, out var userMind) &&
            _role.MindHasRole<RevolutionaryRoleComponent>((userMindId, userMind), out var role))
            role.Value.Comp2.ConvertedCount++;
        // Dumont end

        if (TryComp<ActorComponent>(args.Target, out var actor))
            _antag.SendBriefing(actor.PlayerSession, Loc.GetString("rev-role-greeting"), Color.Red, args.Target.Comp.RevStartSound);

        if (!TryComp<CommandStaffComponent>(args.Target, out var command))
            return;

        command.Enabled = false;
        _rev.CheckCommandLose();
    }
}
