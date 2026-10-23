// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Shared._Trauma.Abductor.Conditions;

/// <summary>
/// Checks the target entity against a whitelist or blacklist.
/// </summary>
public sealed partial class WhitelistCondition : AbductorTaskCondition
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    protected override bool Check(EntityUid target, IEntityManager entMan)
    {
        return entMan.System<EntityWhitelistSystem>().CheckBoth(target, blacklist: Blacklist, whitelist: Whitelist);
    }
}
