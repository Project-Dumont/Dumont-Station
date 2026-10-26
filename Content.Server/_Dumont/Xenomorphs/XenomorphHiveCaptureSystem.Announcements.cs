using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphHiveCaptureSystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private Content.Server.Chat.Managers.IChatManager _hiveChat = default!;

    public void AnnounceHive(string message, SoundSpecifier? sound = null)
    {
        var query = EntityQueryEnumerator<ActorComponent, XenomorphLineageComponent, XenomorphComponent>();
        while (query.MoveNext(out _, out var actor, out _, out _))
        {
            RaiseNetworkEvent(new XenomorphQueenAnnouncementEvent(message), actor.PlayerSession);
            _hiveChat.ChatMessageToOne(ChatChannel.Local, message, FormattedMessage.EscapeText(message), default, false,
                actor.PlayerSession.Channel, Color.FromHex("#b56dd6"), recordReplay: true, hidePopup: true);
        }
        PlayHiveCommandSound(sound);
    }

    public void PlayHiveCommandSound(SoundSpecifier? sound = null)
    {
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<ActorComponent, XenomorphLineageComponent, XenomorphComponent>();
        while (query.MoveNext(out _, out var actor, out _, out _)) filter.AddPlayer(actor.PlayerSession);
        _audio.PlayGlobal(sound ?? new SoundCollectionSpecifier("DumontXenomorphQueenCommand", AudioParams.Default.WithVolume(-6)), filter, true);
    }

}
