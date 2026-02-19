using Content.Shared.Chat;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.Wraith;

public sealed class DarkWhisperSystem : EntitySystem
{
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DarkWhisperComponent, DarkWhisperEvent>(OnDarkWhisper);
        SubscribeLocalEvent<DarkWhisperComponent, EntitySpokeEvent>(OnDarkWhisperSpoke);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<DarkWhisperComponent>();
        while (eqe.MoveNext(out var uid, out var whisper))
        {
            if (!whisper.Active || whisper.AttachedEntity == null)
                return;

            if (_timing.CurTime < whisper.NextUpdate)
                return;

            _popupSystem.PopupClient(Loc.GetString("dark-whisper-end"), uid, PopupType.MediumCaution);

            whisper.Active = false;
            whisper.AttachedEntity = null;
            Dirty(uid, whisper);
        }
    }

    private void OnDarkWhisper(Entity<DarkWhisperComponent> ent, ref DarkWhisperEvent args)
    {
        // popup here
        _popupSystem.PopupClient(Loc.GetString("dark-whisper-start"), ent.Owner, PopupType.MediumCaution);
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.Update;
        ent.Comp.AttachedEntity = args.Target;
        ent.Comp.Active = true;
        Dirty(ent);
    }

    private void OnDarkWhisperSpoke(Entity<DarkWhisperComponent> ent, ref EntitySpokeEvent args)
    {
        if (!ent.Comp.Active || ent.Comp.AttachedEntity is not {} attachedEntity)
            return;

        var message = args.Message;

        _chatSystem.TrySendInGameICMessage(
            attachedEntity,
            message,
            InGameICChatType.Speak,
            false,
            false,
            null,
            null,
            null,
            true,
            true);
    }
}
