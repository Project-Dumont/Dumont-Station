// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._EinsteinEngines.Language;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Trauma.Shared.Language;

namespace Content.Trauma.Server.Language;

public sealed partial class CodespeakManualSystem : EntitySystem
{
    [Dependency] private LanguageSystem _language = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CodespeakManualComponent, UseInHandEvent>(OnUse);
    }

    private void OnUse(Entity<CodespeakManualComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || TryComp<BlindableComponent>(args.User, out var blind) && blind.IsBlind)
            return;

        args.Handled = true;
        _language.AddLanguage(args.User, "Codespeak");
        _popup.PopupEntity(Loc.GetString("codespeak-manual-learned"), ent, args.User);
        if (!ent.Comp.SingleUse)
            return;

        Spawn("Ash", Transform(ent).Coordinates);
        QueueDel(ent);
    }
}
