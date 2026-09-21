using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Coroner;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedCoronerSystem))]
public sealed partial class AutopsyToolComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan TableDuration = TimeSpan.FromSeconds(15);

    [DataField]
    public SoundSpecifier? Sound = new SoundCollectionSpecifier("PaperScribbles");

    [DataField]
    public EntProtoId Report = "Paper";
}

[Serializable, NetSerializable]
public sealed partial class AutopsyDoAfterEvent : SimpleDoAfterEvent;
