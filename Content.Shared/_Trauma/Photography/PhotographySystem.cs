// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityTable;
using Content.Shared.Examine;
using Content.Shared.Flash;
using Content.Shared.Flash.Components;
using Content.Shared.Interaction;
using Content.Shared.Charges.Systems;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Shared.Photography;

/// <summary>
/// Handles everything related to photography.
/// </summary>
public sealed partial class PhotographySystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private EntityTableSystem _tables = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedChargesSystem _charges = default!;
    [Dependency] private UseDelaySystem _delay = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedFlashSystem _flash = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotographComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PictureTakerComponent, BeforeRangedInteractEvent>(OnInteract);
    }

    private void OnExamined(Entity<PhotographComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(PhotographComponent)))
        {
            if (string.IsNullOrEmpty(ent.Comp.NameText))
                args.PushText(Loc.GetString("photograph-name-text-empty"));
            else
                args.PushText(ent.Comp.NameText);
            if (ent.Comp.Description != null)
                // TODO: For some weird reason ExamineSystem is adding a new line at the end of message we are pushing with each examine.
                // I'm not soaping this PR even more, so for now I'll just bandaid that by sending a clone to prevent it from getting modified.
                args.PushMessage(new FormattedMessage(ent.Comp.Description));
        }
    }

    // Use the native flash and charge systems before printing the photo.
    private void OnInteract(Entity<PictureTakerComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (_net.IsClient || args.Handled || !args.CanReach || args.Target == null ||
            _delay.IsDelayed(ent.Owner) || !TryComp<FlashComponent>(ent, out var flash) ||
            !_charges.TryUseCharge(ent.Owner))
            return;

        args.Handled = true;
        _delay.TryResetDelay(ent);
        _audio.PlayPvs(flash.Sound, ent);
        _flash.Flash(args.Target.Value, args.User, ent, flash.MeleeDuration, flash.SlowTo);
        TakePicture(ent, args.Target, args.User);
    }

    /// <summary>
    /// Processes entity aimed at with a camera and prints a picture of it.
    /// TODO: This is basically a placeholder mechanic for a more elaborate photography system.
    /// However, this will need major refactoring to be possible. See
    /// https://github.com/space-wizards/docs/pull/307 and
    /// https://github.com/space-wizards/space-station-14/pull/43327
    /// for details.
    /// </summary>
    public void TakePicture(Entity<PictureTakerComponent> camera, EntityUid? target, EntityUid? user)
    {
        if (_net.IsClient)
            return; // Can't interact with predictively spawned entities yet.

        var tableResult = _tables.GetSpawns(camera.Comp.Photographs);
        var coords = Transform(camera).Coordinates;

        FormattedMessage? description = null;
        string? nameText = null;
        if (target != null)
        {
            description = _examine.GetExamineText(target.Value, user);
            // Get the full string now instead of indexing it later because we need the entity to know if it uses a proper noun or not.
            nameText = Loc.GetString("photograph-name-text", ("entity", Identity.Entity(target.Value, EntityManager)));
            // We don't want photographs to contain the descriptions of other photographs, because that makes entities with, in theory, infinite descriptions.
            if (HasComp<PhotographComponent>(target.Value))
            {
                description = null;
                nameText = Loc.GetString("photograph-name-text-photograph");
            }
        }

        foreach (var prototype in tableResult)
        {
            // we generate an individual photograph (there should be only one tough)
            var spawned = Spawn(prototype, coords);
            var photoComp = EnsureComp<PhotographComponent>(spawned);
            photoComp.NameText = nameText;
            photoComp.Description = description;
            Dirty(spawned, photoComp);

            _hands.PickupOrDrop(user, spawned, dropNear: true);
        }
    }
}
