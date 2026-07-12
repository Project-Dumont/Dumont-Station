namespace Content.Shared.Pda;

public enum NotificationMode {
    All,
    Command,
    Cargo,
    Science,
    Engineering,
    Service,
    Medical
}

public sealed partial class PdaNotificationEvent(string message, NotificationOptions options) : HandledEntityEventArgs {
    public readonly string Message = message;
    public readonly NotificationOptions Options = options;
}

public sealed class NotificationOptions(bool isLoud, NotificationMode mode) {
    public readonly bool IsLoud = isLoud;
    public readonly NotificationMode Mode = mode;
}
