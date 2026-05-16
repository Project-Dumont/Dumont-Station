using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Orion.Bitrunning;

public sealed partial class BitrunningDisconnectAvatarActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class BitrunningDisconnectAvatarDoAfterEvent : SimpleDoAfterEvent;
