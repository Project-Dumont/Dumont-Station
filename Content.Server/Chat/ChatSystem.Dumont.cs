using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Chat.Systems;

// Dumont start
public sealed partial class ChatSystem
{
    public override void PlayEmote(EntityUid source, ProtoId<EmotePrototype> emote)
    {
        TryEmoteWithChat(source, emote);
    }
}
// Dumont end
