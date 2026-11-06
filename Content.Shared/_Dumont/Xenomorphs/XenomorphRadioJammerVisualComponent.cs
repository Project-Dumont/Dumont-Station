// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dumont.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphRadioJammerVisualComponent : Component
{
    [DataField, AutoNetworkedField] public float Radius = 3f;
    [DataField, AutoNetworkedField] public bool Active;
    [DataField, AutoNetworkedField] public float StartScale;
    [DataField, AutoNetworkedField] public TimeSpan FadeTime = TimeSpan.FromMilliseconds(500);
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan TransitionAt;

    public float ScaleAt(TimeSpan now)
    {
        var progress = FadeTime <= TimeSpan.Zero ? 1f
            : (float) Math.Clamp((now - TransitionAt).TotalSeconds / FadeTime.TotalSeconds, 0, 1);
        return StartScale + ((Active ? 1f : 0f) - StartScale) * progress;
    }
}
