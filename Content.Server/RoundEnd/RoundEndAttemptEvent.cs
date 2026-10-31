// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.RoundEnd;

/// <summary>Allows game modes to reject an evacuation request before its timer starts.</summary>
[ByRefEvent]
public struct EvacuationRequestAttemptEvent
{
    public bool Cancelled;
}

/// <summary>Allows an ongoing finale to defer the normal automatic round end.</summary>
[ByRefEvent]
public struct RoundEndAttemptEvent
{
    public bool Cancelled;
}
