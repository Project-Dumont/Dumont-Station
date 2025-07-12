// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kill_Me_I_Noobs <118206719+Vonsant@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;

namespace Content.Shared._CorvaxNext.CrewMedal;

public abstract class SharedCrewMedalSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CrewMedalComponent, ExaminedEvent>(OnExamined);
    }

    /// <summary>
    /// Displays the reason and recipient of an awarded medal during an Examine action.
    /// </summary>
    private void OnExamined(Entity<CrewMedalComponent> medal, ref ExaminedEvent args)
    {
        if (!medal.Comp.Awarded)
            return;

        var text = Loc.GetString(
            "comp-crew-medal-inspection-text",
            ("recipient", medal.Comp.Recipient),
            ("reason", medal.Comp.Reason)
        );

        args.PushMarkup(text);
    }
}
