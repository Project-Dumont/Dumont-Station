using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

[Serializable, NetSerializable]
public sealed partial class DnaInjectorDoAfterEvent : SimpleDoAfterEvent { }
