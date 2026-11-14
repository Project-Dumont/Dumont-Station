// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Trauma.Shared.JobListings;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class CreateJobBoardEvent : EntityEventArgs
{
    [DataField(required: true)]
    public EntProtoId<JobListingsComponent> JobBoardProto;
}
