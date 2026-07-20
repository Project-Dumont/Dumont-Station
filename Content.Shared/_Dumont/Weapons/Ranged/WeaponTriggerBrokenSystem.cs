// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._Dumont.Weapons.Ranged;

/// <summary>
/// Handles ranged weapons with a broken trigger.
/// Prevents firing and allows the trigger to be repaired with a welder.
/// </summary>
public sealed class WeaponTriggerBrokenSystem : EntitySystem
{
    private static readonly string[] WeldingQualities = ["Welding"];

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeaponTriggerBrokenComponent, ShotAttemptedEvent>(
            OnShotAttempted);

        SubscribeLocalEvent<WeaponTriggerBrokenComponent, ExaminedEvent>(
            OnExamined);

        SubscribeLocalEvent<WeaponTriggerBrokenComponent, AfterInteractUsingEvent>(
            OnAfterInteractUsing);

        SubscribeLocalEvent<WeaponTriggerBrokenComponent, RepairBrokenTriggerDoAfterEvent>(
            OnRepairDoAfter);
    }

    /// <summary>
    /// Prevents the weapon from firing and provides feedback to the user.
    /// The click sound only plays when the cooldown allows the popup to appear.
    /// </summary>
    private void OnShotAttempted(
        Entity<WeaponTriggerBrokenComponent> ent,
        ref ShotAttemptedEvent args)
    {
        var currentTime = _timing.CurTime;
        var nextPopupTime =
            ent.Comp.LastPopupTime +
            ent.Comp.PopupCooldown;

        if (currentTime >= nextPopupTime)
        {
            ent.Comp.LastPopupTime = currentTime;

            _popup.PopupPredicted(
                Loc.GetString("weapon-trigger-broken-fire-popup"),
                args.User,
                args.User,
                PopupType.SmallCaution);

            _audio.PlayPredicted(
                ent.Comp.ClickSound,
                ent.Owner,
                args.User);
        }

        args.Cancel();
    }

    /// <summary>
    /// Adds a warning to the weapon's examination text.
    /// </summary>
    private void OnExamined(
        Entity<WeaponTriggerBrokenComponent> ent,
        ref ExaminedEvent args)
    {
        args.PushMarkup(
            Loc.GetString("weapon-trigger-broken-examine"));
    }

    /// <summary>
    /// Begins repairing the trigger when an enabled welding tool is used.
    /// Welding sound and sparks are handled by WeldingSparksSystem through
    /// the UseToolEvent raised by SharedToolSystem.
    /// </summary>
    private void OnAfterInteractUsing(
        Entity<WeaponTriggerBrokenComponent> ent,
        ref AfterInteractUsingEvent args)
    {
        if (args.Handled || args.Target != ent.Owner)
            return;

        if (!TryComp<WelderComponent>(args.Used, out var welder))
            return;

        if (!welder.Enabled)
            return;

        var started = _toolSystem.UseTool(
            args.Used,
            args.User,
            ent.Owner,
            (float)ent.Comp.RepairDuration.TotalSeconds,
            WeldingQualities,
            new RepairBrokenTriggerDoAfterEvent());

        if (!started)
            return;

        args.Handled = true;
    }

    /// <summary>
    /// Finishes the repair after the tool do-after completes.
    /// </summary>
    private void OnRepairDoAfter(
        Entity<WeaponTriggerBrokenComponent> ent,
        ref RepairBrokenTriggerDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (args.Used is not { } used)
            return;

        if (!TryComp<WelderComponent>(used, out var welder))
            return;

        if (!welder.Enabled)
            return;

        _popup.PopupPredicted(
            Loc.GetString("weapon-trigger-broken-repaired-popup"),
            args.User,
            args.User,
            PopupType.Small);

        if (_net.IsServer)
            RemCompDeferred<WeaponTriggerBrokenComponent>(ent.Owner);
    }
}

/// <summary>
/// Raised when the broken-trigger repair do-after finishes.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class RepairBrokenTriggerDoAfterEvent : SimpleDoAfterEvent
{
}
