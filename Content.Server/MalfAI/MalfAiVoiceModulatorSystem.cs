// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Content.Shared.MalfAI;
using Content.Shared.Preferences;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Content.Shared.Popups;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Localization;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using Content.Server.Silicons.StationAi;

namespace Content.Server.MalfAI;

public sealed class MalfAiVoiceModulatorSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;

    // CCVar.
    private int _maxNameLenght;

    public override void Initialize()
    {
        base.Initialize();
        // Open the Malf voice modulator UI on action use.
        SubscribeLocalEvent<MalfAiVoiceModulatorActionEvent>(OnVoiceModulator);
        // Receive the chosen name from the client and apply it server-side.
        SubscribeNetworkEvent<MalfVoiceModulatorSubmitNameEvent>(OnSubmitName);

        Subs.CVar(_cfgManager, CCVars.MaxNameLength, value => _maxNameLenght = value, true);
    }

    private void OnVoiceModulator(MalfAiVoiceModulatorActionEvent ev)
    {
        // Find the player session to target their client with the UI open event.
        if (!TryComp<ActorComponent>(ev.Performer, out var actor))
            return;

        // Resolve the AI core (ensure only AI can use it).
        var core = SharedMalfAiHelpers.ResolveAiCoreFrom(EntityManager, _xform, ev.Performer);
        if (core == EntityUid.Invalid)
            return;

        // Open the client-side Malf voice modulator window.
        RaiseNetworkEvent(new MalfVoiceModulatorOpenUiEvent(), Filter.SinglePlayer(actor.PlayerSession));
    }

    private void OnSubmitName(MalfVoiceModulatorSubmitNameEvent ev, EntitySessionEventArgs args)
    {
        // Identify the sender's controlled entity.
        var performer = args.SenderSession.AttachedEntity;
        if (performer == null)
            return;

        var popupTarget = GetAiEyeForPopup(performer.Value) ?? performer.Value;

        // Resolve the AI core for this performer (validation only).
        var core = SharedMalfAiHelpers.ResolveAiCoreFrom(EntityManager, _xform, performer.Value);
        if (core == EntityUid.Invalid)
            return;

        // Validate and apply the new name to the controlled entity (positronic brain).
        var newName = ev.Name.Trim();

        if (string.IsNullOrEmpty(newName) || newName.Length > _maxNameLenght)
        {
            _popup.PopupEntity(Loc.GetString("malf-voice-invalid-name"), popupTarget, performer.Value, PopupType.SmallCaution);
            return;
        }

        _meta.SetEntityName(performer.Value, newName);

        // Admin log for auditability.
        _adminLog.Add(LogType.Action, LogImpact.Medium, $"AI voice modulator: {ToPrettyString(performer.Value)} set name to \"{newName}\"");
        _popup.PopupEntity(Loc.GetString("malf-voice-updated"), popupTarget, performer.Value);
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }
}
