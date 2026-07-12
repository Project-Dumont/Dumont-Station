// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Robust.Shared.GameStates;

namespace Content.Shared._Harmony.Roles.Components;

/// <summary>
/// Added to mind role entities to tag that they are a conspirator.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ConspiratorRoleComponent : BaseMindRoleComponent;
