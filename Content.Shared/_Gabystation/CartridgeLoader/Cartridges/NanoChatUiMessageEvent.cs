using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public enum NanoBankUiMessageType : byte
{
    Empty,
    Login,
    Logout,
    Transfer,
    ToggleMute,
}

// Separar em diferentes messages
[Serializable, NetSerializable]
public sealed class NanoBankUiMessageEvent : CartridgeMessageEvent
{
    public readonly NanoBankUiMessageType Type;

    public readonly int? TargetAccount;

    public readonly int? Content;
    public NanoBankUiMessageEvent(NanoBankUiMessageType type,
        int? targetAccount = null,
        int? content = null)
    {
        Type = type;
        TargetAccount = targetAccount;
        Content = content;
    }
}
