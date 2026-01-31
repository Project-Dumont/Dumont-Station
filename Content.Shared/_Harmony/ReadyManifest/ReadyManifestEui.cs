// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Eui;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Harmony.ReadyManifest;

[Serializable, NetSerializable]
public sealed class ReadyManifestEuiState : EuiStateBase
{
    public Dictionary<ProtoId<JobPrototype>, ReadyManifestJobData> JobCounts { get; }

    public ReadyManifestEuiState(Dictionary<ProtoId<JobPrototype>, ReadyManifestJobData> jobCounts)
    {
        JobCounts = jobCounts;
    }
}
