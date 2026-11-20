// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction;
using Content.Shared.Construction.Steps;
using Content.Shared.Examine;
using Robust.Shared.Utility;

namespace Content.Shared._Dumont.Construction;

public sealed partial class EmagConstructionGraphStep : ConstructionGraphStep
{
    [DataField("emag", required: true)]
    public LocId Instructions;

    public override void DoExamine(ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(Instructions));
    }

    public override ConstructionGuideEntry GenerateGuideEntry()
    {
        return new ConstructionGuideEntry
        {
            Localization = Instructions,
            Icon = new SpriteSpecifier.Rsi(new("Objects/Tools/emag.rsi"), "icon"),
        };
    }
}
