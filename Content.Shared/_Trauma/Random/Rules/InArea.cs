// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Random.Rules;
using Content.Shared.Whitelist;
using Content.Trauma.Shared.Areas;

namespace Content.Trauma.Shared.Random.Rules;

/// <summary>
/// Returns true if the attached entity is inside an area, and the area matches the whitelist and blacklist if set.
/// </summary>
public sealed partial class InAreaRule : RulesRule
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        var areaSys = entManager.System<AreaSystem>();
        var whitelist = entManager.System<EntityWhitelistSystem>();

        return (areaSys.GetArea(uid) is { } area
               && whitelist.CheckBoth(area, Blacklist, Whitelist))
               != Inverted;
    }
}
