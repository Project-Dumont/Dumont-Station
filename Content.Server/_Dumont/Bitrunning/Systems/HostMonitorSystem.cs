// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Orion.Bitrunning;
using Content.Shared._Orion.Bitrunning.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Interaction;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Robust.Shared.Utility;

namespace Content.Server._Orion.Bitrunning.Systems;

public sealed class HostMonitorSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HostMonitorComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<HostMonitorComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<HostMonitorComponent, GetVerbsEvent<AlternativeVerb>>(OnAltVerb);
    }

    private void OnExamine(EntityUid uid, HostMonitorComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var modeName = component.Mode == HostMonitorMode.Integrity
            ? Loc.GetString("host-monitor-mode-integrity")
            : Loc.GetString("host-monitor-mode-objective");

        args.PushMarkup(Loc.GetString("host-monitor-mode", ("mode", modeName)));
    }

    private void OnAltVerb(EntityUid uid, HostMonitorComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var nextMode = component.Mode == HostMonitorMode.Integrity
            ? HostMonitorMode.Objective
            : HostMonitorMode.Integrity;
        var modeName = nextMode == HostMonitorMode.Integrity
            ? Loc.GetString("host-monitor-mode-integrity")
            : Loc.GetString("host-monitor-mode-objective");

        AlternativeVerb verb = new()
        {
            Act = () => SetMode(uid, component, nextMode, args.User),
            Text = Loc.GetString("host-monitor-mode", ("mode", modeName)),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/fold.svg.192dpi.png")),
        };

        args.Verbs.Add(verb);
    }

    private void SetMode(EntityUid uid, HostMonitorComponent component, HostMonitorMode mode, EntityUid user)
    {
        if (component.Mode == mode)
            return;

        component.Mode = mode;
        Dirty(uid, component);

        var modeName = mode == HostMonitorMode.Integrity
            ? Loc.GetString("host-monitor-mode-integrity")
            : Loc.GetString("host-monitor-mode-objective");

        _popup.PopupEntity(Loc.GetString("host-monitor-mode", ("mode", modeName)), uid, user);
    }

    private void OnActivate(EntityUid uid, HostMonitorComponent component, ActivateInWorldEvent args)
    {
        if (TryComp<AvatarConnectionComponent>(args.User, out var avatar) && avatar.OriginalBody is { } hostBody && Exists(hostBody) && TryComp<DamageableComponent>(hostBody, out var damageable) && _mobThreshold.TryGetIncapThreshold(hostBody, out var critThreshold) && critThreshold.Value > 0)
        {
            if (component.Mode == HostMonitorMode.Integrity)
            {
                var percentage = Math.Clamp((int) ((1f - _mobThreshold.CheckVitalDamage(hostBody, damageable).Float() / critThreshold.Value.Float()) * 100), 0, 100);
                var message = Loc.GetString("host-monitor-health", ("percentage", percentage));
                _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, hideChat: true);
            }
            else
            {
                if (avatar.Server is { } serverUid && TryComp<QuantumServerComponent>(serverUid, out var server))
                {
                    var message = server.ObjectiveCompleted
                        ? Loc.GetString("host-monitor-objective-completed")
                        : Loc.GetString("host-monitor-objective-report",
                            ("objective", Loc.GetString(server.ObjectiveType switch
                            {
                                BitrunningObjectiveType.CollectEncryptedCaches => "host-monitor-objective-collect",
                                BitrunningObjectiveType.DeliveryCacheCrate => "host-monitor-objective-delivery",
                                BitrunningObjectiveType.EliminateEnemies => "host-monitor-objective-eliminate",
                                BitrunningObjectiveType.CatchFish => "host-monitor-objective-fish",
                                BitrunningObjectiveType.FillStomach => "host-monitor-objective-stomach",
                                BitrunningObjectiveType.OverhydrateStomach => "host-monitor-objective-hydrate",
                                _ => "host-monitor-objective-none"
                            })),
                            ("points", server.ObjectivePoints),
                            ("goal", server.ObjectiveGoal));
                    _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, hideChat: true);
                }
                else
                {
                    var message = Loc.GetString("host-monitor-error");
                    _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, hideChat: true);
                }
            }
        }
        else
        {
            var message = Loc.GetString("host-monitor-error");
            _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, hideChat: true);
        }

        args.Handled = true;
    }
}
