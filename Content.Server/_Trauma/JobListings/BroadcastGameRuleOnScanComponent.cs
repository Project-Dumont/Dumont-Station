// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Trauma.Server.JobListings;

[RegisterComponent]
public sealed partial class BroadcastGameRuleOnScanComponent : Component
{
    [DataField(required: true)]
    public EntProtoId GameRule;
}
