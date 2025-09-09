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

[Serializable, NetSerializable]
public sealed class NanoBankUiMessageEvent : CartridgeMessageEvent
{
    public readonly NanoBankUiMessageType Type;

    public readonly int? TargetAccount;

    public readonly float? Content;
    public NanoBankUiMessageEvent(NanoBankUiMessageType type,
        int? targetAccount = null,
        float? content = null)
    {
        Type = type;
        TargetAccount = targetAccount;
        Content = content;
    }
}
