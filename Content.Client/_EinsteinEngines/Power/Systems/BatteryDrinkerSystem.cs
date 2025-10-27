// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 X <70487315+XWasHere@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Power.Components;
using Content.Shared._EinsteinEngines.Power.Systems;
using Content.Shared._EinsteinEngines.Silicon.Charge;
using Content.Shared.PowerCell.Components;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Utility;

namespace Content.Client._EinsteinEngines.Power.Systems;

// Goobstation - Energycrit
/// <summary>
///     Client-side prediction for BatteryDrinkerSystem.
/// </summary>
/// <remarks>
///     For some reason, the battery drinking system has a feature letting you drink from anything
///     with a BatteryComponent, this means that all the logic for figuring out what you can drink
///     or not has to be entirely serverside. The feature isn't even used anywhere. It was briefly
///     going to be used in energycrit, but I very quickly realized that it was incredibly broken
///     and shouldn't have existed in the first place. Because of this, the logic has to be copied
///     and shoved into the client too!
/// </remarks>
public sealed class BatteryDrinkerSystem : SharedBatteryDrinkerSystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryDrinkerSourceComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<PowerCellSlotComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs<TComp>(Entity<TComp> ent, ref GetVerbsEvent<AlternativeVerb> args)
        where TComp : Component
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<BatteryDrinkerComponent>(args.User, out var drinker) ||
            _whitelist.IsBlacklistPass(drinker.Blacklist, ent) ||
            !SearchForDrinker(args.User, out _) ||
            !SearchForSource(ent, out _))
            return;

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("battery-drinker-verb-drink"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/smite.svg.192dpi.png")),
            Priority = -5
        };

        args.Verbs.Add(verb);
    }
}
