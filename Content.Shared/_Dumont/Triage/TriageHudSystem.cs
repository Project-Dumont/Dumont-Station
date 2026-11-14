// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Shared._Dumont.Overlays;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._Dumont.Triage;

/// <summary>
/// Quem usa medhud pode dar alt-click (ou clicar com o botão direito e escolher "Mudar triagem") num paciente
/// para abrir a janela de triagem, sem precisar do aparelho de triagem na mão
/// </summary>
public sealed partial class TriageHudSystem : EntitySystem
{
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerbs);
    }

    /// <summary>
    /// Procura o medhud de quem está olhando: no próprio corpo (olho cibernético, borg) ou em algo vestido
    /// Usa as mesmas posições que o jogo usa para desenhar os ícones
    /// </summary>
    public bool TryGetHud(EntityUid user, [NotNullWhen(true)] out ShowTriageIconsComponent? hud)
    {
        if (TryComp(user, out hud))
            return true;

        if (TryComp<InventoryComponent>(user, out var inventory))
        {
            var slots = _inventory.GetSlotEnumerator((user, inventory), ~SlotFlags.POCKET);

            while (slots.MoveNext(out var slot))
            {
                if (slot.ContainedEntity is { } item && TryComp(item, out hud))
                    return true;
            }
        }

        hud = null;
        return false;
    }

    // Mesmo formato do IdExaminableSystem (sechud)
    private void OnGetAlternativeVerbs(EntityUid uid, MobStateComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Só quem tem a janela de triagem no corpo (as espécies) e só com medhud
        if (!_ui.HasUi(uid, TriageHudUiKey.Key) || !TryGetHud(args.User, out _))
            return;

        var user = args.User;
        var target = args.Target;

        // Prioridade maior que carregar e acordar (2) para o alt-click abrir a triagem
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("triage-hud-verb"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/plus.svg.192dpi.png")),
            Priority = 4,
            Act = () => _ui.TryOpenUi(target, TriageHudUiKey.Key, user),
        });
    }
}
