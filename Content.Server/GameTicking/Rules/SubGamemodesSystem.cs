// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Storage;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules;

public sealed class SubGamemodesSystem : GameRuleSystem<SubGamemodesComponent>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private string _ruleCompName = default!;

    public override void Initialize()
    {
        base.Initialize();
        _ruleCompName = Factory.GetComponentName<GameRuleComponent>();
    }

    protected override void Added(EntityUid uid, SubGamemodesComponent comp, GameRuleComponent rule, GameRuleAddedEvent args)
    {
        var candidates = comp.Rules;

        if (_cfg.GetCVar(GoobCVars.SecretUseOnlinePlayerCount))
        {
            var players = GameTicker.OnlinePlayerCount();
            candidates = comp.Rules.Where(entry => CanRun(entry, players)).ToList();
        }

        var picked = EntitySpawnCollection.GetSpawns(candidates, RobustRandom);
        foreach (var id in picked)
        {
            Log.Info($"Starting gamerule {id} as a subgamemode of {ToPrettyString(uid):rule}");
            GameTicker.AddGameRule(id);
        }
    }

    private bool CanRun(EntitySpawnEntry entry, int players)
    {
        if (entry.PrototypeId is not { } protoId)
            return true;

        if (!_proto.TryIndex(protoId, out EntityPrototype? ruleProto)
            || !ruleProto.TryGetComponent(_ruleCompName, out GameRuleComponent? ruleComp))
            return true;

        return ruleComp.MinPlayers <= players || !ruleComp.CancelPresetOnTooFewPlayers;
    }
}
