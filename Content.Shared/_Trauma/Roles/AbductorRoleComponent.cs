// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;

namespace Content.Shared._Trauma.Roles;

[RegisterComponent]
public sealed partial class AbductorRoleComponent : BaseMindRoleComponent
{
    /// <summary>
    /// The gamerule this abductor was made by.
    /// </summary>
    [DataField]
    public EntityUid? Rule;
}
