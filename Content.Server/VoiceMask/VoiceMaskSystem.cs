// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Bakke <luringens@protonmail.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 āda <ss.adasts@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.IntrinsicVoiceModulator.VoiceMask; // Goobstation
using Content.Shared.Actions;
using Content.Shared.Implants;
using Content.Shared.IdentityManagement.Components;
using Content.Server.IdentityManagement;
using Content.Server.Access.Systems;
using Content.Server.Access.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Chat.RadioIconsEvents; // Goobstation
using Content.Shared.Clothing;
using Content.Shared.Database;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Content.Shared.Roles.Jobs; // Goobstation
using Content.Shared.Speech;
using Content.Shared.VoiceMask;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server.VoiceMask;

public sealed partial class VoiceMaskSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private IConfigurationManager _cfgManager = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SharedActionsSystem _actions = default!;

    [Dependency] private IdentitySystem _identity = default!;
    [Dependency] private IdCardSystem _id = default!;

    // CCVar.
    private int _maxNameLength;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VoiceMaskComponent, ImplantRelayEvent<TransformSpeakerNameEvent>>(OnImplantVoice);
        SubscribeLocalEvent<VoiceMaskComponent, ImplantRelayEvent<SeeIdentityAttemptEvent>>(OnImplantIdentity);
        SubscribeLocalEvent<VoiceMaskComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<VoiceMaskComponent, ImplantRemovedEvent>(OnImplantRemoved);
        SubscribeLocalEvent<VoiceMaskComponent, InventoryRelayedEvent<TransformSpeakerNameEvent>>(OnTransformSpeakerName);
        SubscribeLocalEvent<VoiceMaskComponent, VoiceMaskChangeNameMessage>(OnChangeName);
        SubscribeLocalEvent<VoiceMaskComponent, VoiceMaskChangeVerbMessage>(OnChangeVerb);
        SubscribeLocalEvent<VoiceMaskComponent, ClothingGotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<VoiceMaskSetNameEvent>(OpenUI);

        Subs.CVar(_cfgManager, CCVars.MaxNameLength, value => _maxNameLength = value, true);
    }

    private void OnTransformSpeakerName(Entity<VoiceMaskComponent> entity, ref InventoryRelayedEvent<TransformSpeakerNameEvent> args)
    {
        args.Args.VoiceName = GetCurrentVoiceName(entity);
        args.Args.SpeechVerb = entity.Comp.VoiceMaskSpeechVerb ?? args.Args.SpeechVerb;
    }

    private void OnImplantVoice(Entity<VoiceMaskComponent> ent, ref ImplantRelayEvent<TransformSpeakerNameEvent> args)
    {
        args.Event.VoiceName = GetCurrentVoiceName(ent);
        args.Event.SpeechVerb = ent.Comp.VoiceMaskSpeechVerb ?? args.Event.SpeechVerb;
    }

    private void OnImplantIdentity(Entity<VoiceMaskComponent> ent, ref ImplantRelayEvent<SeeIdentityAttemptEvent> args)
    {
        if (ent.Comp.OverrideIdentity)
            args.Event.NameOverride = GetCurrentVoiceName(ent);
    }

    private void OnImplanted(Entity<VoiceMaskComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted is {} wearer)
            _identity.QueueIdentityUpdate(wearer);
    }

    private void OnImplantRemoved(Entity<VoiceMaskComponent> ent, ref ImplantRemovedEvent args)
    {
        _identity.QueueIdentityUpdate(args.Implanted);
    }

    #region User inputs from UI
    private void OnChangeVerb(Entity<VoiceMaskComponent> entity, ref VoiceMaskChangeVerbMessage msg)
    {
        if (msg.Verb is { } id && !_proto.HasIndex<SpeechVerbPrototype>(id))
            return;

        entity.Comp.VoiceMaskSpeechVerb = msg.Verb;
        // verb is only important to metagamers so no need to log as opposed to name

        _popupSystem.PopupEntity(Loc.GetString("voice-mask-popup-success"), entity, msg.Actor);

        UpdateUI(entity);
    }

    private void OnChangeName(Entity<VoiceMaskComponent> entity, ref VoiceMaskChangeNameMessage message)
    {
        if (message.Name.Length > _maxNameLength || message.Name.Length <= 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("voice-mask-popup-failure"), entity, message.Actor, PopupType.SmallCaution);
            return;
        }

        entity.Comp.VoiceMaskName = message.Name;
        if (entity.Comp.ChangeIDName && _id.TryFindIdCard(message.Actor, out var card) && HasComp<AgentIDCardComponent>(card))
            _id.TryChangeFullName(card, message.Name, card.Comp, message.Actor);
        if (entity.Comp.OverrideIdentity)
            _identity.QueueIdentityUpdate(message.Actor);
        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(message.Actor):player} set voice of {ToPrettyString(entity):mask}: {entity.Comp.VoiceMaskName}");

        _popupSystem.PopupEntity(Loc.GetString("voice-mask-popup-success"), entity, message.Actor);

        UpdateUI(entity);
    }

    #endregion

    #region UI
    private void OnEquip(EntityUid uid, VoiceMaskComponent component, ClothingGotEquippedEvent args)
    {
        if (component.EnableAction) //Goobstation
            _actions.AddAction(args.Wearer, ref component.ActionEntity, component.Action, uid);
    }

    private void OpenUI(VoiceMaskSetNameEvent ev)
    {
        var maskEntity = ev.Action.Comp.Container;

        if (!TryComp<VoiceMaskComponent>(maskEntity, out var voiceMaskComp))
            return;

        if (!_uiSystem.HasUi(maskEntity.Value, VoiceMaskUIKey.Key))
            return;

        _uiSystem.OpenUi(maskEntity.Value, VoiceMaskUIKey.Key, ev.Performer);
        UpdateUI((maskEntity.Value, voiceMaskComp));
    }

    public void UpdateUI(Entity<VoiceMaskComponent> entity) // Make public by goobstation
    {
        if (_uiSystem.HasUi(entity, VoiceMaskUIKey.Key))
            _uiSystem.SetUiState(entity.Owner, VoiceMaskUIKey.Key, new VoiceMaskBuiState(GetCurrentVoiceName(entity), entity.Comp.VoiceMaskSpeechVerb, entity.Comp.JobIconProtoId)); // GabyStation -> Radio icons
    }
    #endregion

    #region Helper functions
    private string GetCurrentVoiceName(Entity<VoiceMaskComponent> entity)
    {
        return entity.Comp.VoiceMaskName ?? Loc.GetString("voice-mask-default-name-override");
    }
    #endregion
}
