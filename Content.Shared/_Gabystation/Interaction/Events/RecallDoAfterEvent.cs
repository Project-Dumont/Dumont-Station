using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.Interaction.Events;

[Serializable, NetSerializable]
public sealed partial class BindRecallDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class UnbindRecallDoAfterEvent : SimpleDoAfterEvent
{
}
