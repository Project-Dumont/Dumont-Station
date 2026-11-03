// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.EntityEffects.Conditions;

/// <summary>
/// Condition that requires the target entity matches a whitelist and does not match a blacklist.
/// </summary>
public sealed partial class WhitelistCondition : EntityEffectCondition
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    public override bool Condition(EntityEffectBaseArgs args)
        => args.EntityManager.System<EntityWhitelistSystem>().CheckBoth(args.TargetEntity, Blacklist, Whitelist);

    public override string GuidebookExplanation(IPrototypeManager prototype)
        => string.Empty;
}
