// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Dumont.Weapons.Ranged;

/// <summary>
/// Breaks weapon triggers when users with configured species tags attempt
/// to fire them.
/// </summary>
public sealed class SpeciesRestrictedTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeciesRestrictedTriggerComponent, ShotAttemptedEvent>(
            OnShotAttempted);
    }

    private void OnShotAttempted(
        Entity<SpeciesRestrictedTriggerComponent> ent,
        ref ShotAttemptedEvent args)
    {
        // The mechanic is disabled for this weapon.
        if (!ent.Comp.Enabled)
            return;

        // The trigger is already broken. Let WeaponTriggerBrokenSystem handle
        // the failed shot and its normal feedback.
        if (HasComp<WeaponTriggerBrokenComponent>(ent.Owner))
            return;

        // An empty list means that no species are restricted.
        if (ent.Comp.RestrictedSpecies.Count == 0)
            return;

        // The user must possess at least one configured species tag.
        // Convert the list of species IDs to tag IDs before checking.
        var restrictedTags = new List<ProtoId<TagPrototype>>(ent.Comp.RestrictedSpecies.Count);
        foreach (var species in ent.Comp.RestrictedSpecies)
        {
            // Convert the ProtoId<SpeciesPrototype> to a string ID and build a Tag ProtoId.
            restrictedTags.Add(new ProtoId<TagPrototype> { Id = species.AsType() });
        }

        if (!_tagSystem.HasAnyTag(args.User, restrictedTags))
            return;

        // Prevent the current shot.
        args.Cancel();

        var currentTime = _timing.CurTime;
        var nextPopupTime =
            ent.Comp.LastPopup +
            ent.Comp.PopupCooldown;

        if (currentTime >= nextPopupTime)
        {
            ent.Comp.LastPopup = currentTime;

            _popup.PopupPredicted(
                Loc.GetString("species-restricted-trigger-break-popup"),
                args.User,
                args.User,
                PopupType.SmallCaution);

            _audio.PlayPredicted(
                ent.Comp.BreakSound,
                ent.Owner,
                args.User);
        }

        // Add the existing broken-trigger mechanic to the gun.
        var brokenComp = EnsureComp<WeaponTriggerBrokenComponent>(ent.Owner);
        brokenComp.LastPopupTime = currentTime;
    }
}
