// <Trauma>
// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Content.Server._EinsteinEngines.Language;
using Content.Server.Chat.Systems;
using Content.Shared.Speech;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffectNew.Components;
using Content.Server.Speech.EntitySystems;
// </Trauma>
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;

namespace Content.Server.Speech.Muting;

/// <summary>
/// Handles the speech restrictions imposed by <see cref="MutedStatusEffectComponent"/>.
/// </summary>
public sealed partial class MutedStatusEffectSystem : EntitySystem
{
    // <Trauma>
    [Dependency] private LanguageSystem _languages = default!;
    // </Trauma>
    [Dependency] private SharedPopupSystem _popup = default!;

    /// <inheritdoc />
    // Dumont start
    [Dependency] private StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StatusEffectContainerComponent, SpeakAttemptEvent>(RelaySpeak);
        SubscribeLocalEvent<StatusEffectContainerComponent, EmoteEvent>(RelayEmote,
            before: new[] { typeof(VocalSystem), typeof(MumbleAccentSystem) });
        SubscribeLocalEvent<StatusEffectContainerComponent, ScreamActionEvent>(RelayScream,
            before: new[] { typeof(VocalSystem) });
    }

    private void RelaySpeak(EntityUid uid, StatusEffectContainerComponent comp, SpeakAttemptEvent args)
    {
        if (!_status.TryEffectsWithComp<MutedStatusEffectComponent>(uid, out var effects))
            return;
        foreach (var effect in effects)
        {
            var ev = new StatusEffectRelayedEvent<SpeakAttemptEvent>(args, uid);
            OnSpeakAttempt((effect.Owner, effect.Comp1), ref ev);
        }
    }

    private void RelayEmote(EntityUid uid, StatusEffectContainerComponent comp, ref EmoteEvent args)
    {
        if (!_status.TryEffectsWithComp<MutedStatusEffectComponent>(uid, out var effects))
            return;
        foreach (var effect in effects)
        {
            var ev = new StatusEffectRelayedEvent<EmoteEvent>(args, uid);
            OnEmote((effect.Owner, effect.Comp1), ref ev);
        }
    }

    private void RelayScream(EntityUid uid, StatusEffectContainerComponent comp, ScreamActionEvent args)
    {
        if (!_status.TryEffectsWithComp<MutedStatusEffectComponent>(uid, out var effects))
            return;
        foreach (var effect in effects)
        {
            var ev = new StatusEffectRelayedEvent<ScreamActionEvent>(args, uid);
            OnEmoteAction((effect.Owner, effect.Comp1), ref ev);
        }
    }
    // Dumont end

    private void OnEmote(Entity<MutedStatusEffectComponent> ent, ref StatusEffectRelayedEvent<EmoteEvent> args)
    {
        if (args.Args.Handled)
            return;

        // Still leaves the text so it looks like they are pantomiming a laugh.
        if (args.Args.Emote.Category.HasFlag(EmoteCategory.Vocal))
        {
            args.Args.Handled = true;
        }
    }

    private void OnEmoteAction(Entity<MutedStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ScreamActionEvent> args)
    {
        if (args.Args.Handled)
            return;

        _popup.PopupEntity(Loc.GetString(ent.Comp.ActionPopup), args.AppliedTo, args.AppliedTo);
        args.Args.Handled = true;
    }

    private void OnSpeakAttempt(Entity<MutedStatusEffectComponent> ent, ref StatusEffectRelayedEvent<SpeakAttemptEvent> args)
    {
        if (args.Args.Cancelled)
            return;

        var target = args.Args.Uid;
        // <Trauma>
        var language = _languages.GetLanguage(target);
        if (!language.SpeechOverride.RequireSpeech)
            return; // Cannot mute if there's no speech involved
        // </Trauma>

        _popup.PopupEntity(Loc.GetString(ent.Comp.SpeakPopup), target, target);

        args.Args.Cancel();
    }
}
