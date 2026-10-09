// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

// Dumont start
using Content.Shared.Roles;
// Dumont end

namespace Content.Trauma.Shared.Roles;

[RegisterComponent]
public sealed partial class HereticArenaParticipantRoleComponent : BaseMindRoleComponent;
