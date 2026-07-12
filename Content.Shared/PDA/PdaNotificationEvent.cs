using Robust.Shared.Prototypes;

namespace Content.Shared.PDA;

public sealed partial class PdaNotificationEvent(string message, NotificationOptions options) : HandledEntityEventArgs {
    public readonly string Message = message;
    public readonly NotificationOptions Options = options;
}

public sealed class NotificationOptions(bool isLoud, ProtoId<NotificationGroupPrototype> group) {
    public readonly bool IsLoud = isLoud;
    public readonly ProtoId<NotificationGroupPrototype> Group = group;
}
