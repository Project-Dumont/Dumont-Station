// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared._Trauma.Xenomorph;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class QueenRoarComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? RoarActionEntity;

    [DataField]
    public EntProtoId RoarAction = "ActionQueenroar";

    [DataField]
    public SoundSpecifier? SoundRoar = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_screech.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f).WithMaxDistance(15f),
    };

    [DataField]
    public SoundSpecifier? SoundRoarStart = new SoundPathSpecifier("/Audio/_Trauma/Effects/queenroarstart.ogg")
    {
        Params = AudioParams.Default.WithVolume(12f).WithMaxDistance(15f),
    };

    [DataField]
    public float RoarRange = 6f;

    [DataField]
    public TimeSpan RoarStunTime = TimeSpan.FromSeconds(6);

    [DataField]
    public TimeSpan RoarDelay = TimeSpan.FromSeconds(3);
}
