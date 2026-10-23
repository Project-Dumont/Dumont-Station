// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Trauma.Mindcontrol;

[RegisterComponent]
public sealed partial class TimedMindControlComponent : Component
{
    [DataField]
    public TimeSpan ExpiresAt;
}
