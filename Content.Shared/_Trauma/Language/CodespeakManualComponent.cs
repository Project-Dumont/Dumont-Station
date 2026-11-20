// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.Language;

[RegisterComponent]
public sealed partial class CodespeakManualComponent : Component
{
    [DataField]
    public bool SingleUse = true;
}
