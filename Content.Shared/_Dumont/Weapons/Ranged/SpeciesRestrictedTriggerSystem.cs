using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
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
        if (!_tagSystem.HasAnyTag(args.User, ent.Comp.RestrictedSpecies))
            return;

        // Prevent the current shot.
        args.Cancel();

        var currentTime = _timing.CurTime;
        var nextPopupTime =
            ent.Comp.LastPopup +
            TimeSpan.FromSeconds(ent.Comp.PopupCooldown);

        if (currentTime >= nextPopupTime)
        {
            ent.Comp.LastPopup = currentTime;

            _popup.PopupClient(
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
        EnsureComp<WeaponTriggerBrokenComponent>(ent.Owner);
    }
}
