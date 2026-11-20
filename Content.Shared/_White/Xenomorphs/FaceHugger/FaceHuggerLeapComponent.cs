// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared._White.Xenomorphs.FaceHugger;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FaceHuggerLeapComponent : Component
{
    [DataField]
    public EntityUid? LeapActionEntity;

    [DataField]
    public EntProtoId LeapAction = "ActionFaceHuggerLeap";

    [DataField]
    public float LeapSpeed = 6f;

    [DataField]
    public SoundSpecifier? LeapSound = new SoundPathSpecifier("/Audio/Animals/Blob/blobattack.ogg");

    [DataField, AutoNetworkedField]
    public bool IsLeaping;
}
