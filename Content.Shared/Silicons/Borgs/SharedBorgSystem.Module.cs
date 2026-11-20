// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Examine;
using Content.Shared.Localizations;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Shared.Silicons.Borgs;

public abstract partial class SharedBorgSystem
{
    public void InitializeModule()
    {
        SubscribeLocalEvent<BorgModuleComponent, ExaminedEvent>(OnModuleExamine);
        SubscribeLocalEvent<BorgModuleWhitelistComponent, ExaminedEvent>(OnWhitelistExamine);
    }

    private void OnModuleExamine(Entity<BorgModuleComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(BorgModuleComponent)))
        {
            if (TryFormatList(ent.Comp.BorgFitTypes, "borg-module-fit", "types", out var list))
                args.PushMarkup(list);
        }
    }

    private void OnWhitelistExamine(Entity<BorgModuleWhitelistComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.WhitelistInfo is null)
            return;

        using (args.PushGroup(nameof(BorgModuleComponent), 1))
        {
            args.PushMarkup(Loc.GetString(ent.Comp.WhitelistInfo));
        }
    }

    private bool TryFormatList(List<LocId>? list, string messageId, string listId, [NotNullWhen(true)] out string? formattedList)
    {
        formattedList = null;

        if (list == null || list.Count == 0)
            return false;

        var entries = ContentLocalizationManager.FormatList([.. list.Select(s => Loc.GetString(s))]);

        formattedList = Loc.GetString(messageId, (listId, entries));
        return true;
    }
}
