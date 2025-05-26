using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.Weapons.Ranged.Events;

[Serializable, NetSerializable]
public sealed partial class UnjamDoAfterEvent : SimpleDoAfterEvent { }
