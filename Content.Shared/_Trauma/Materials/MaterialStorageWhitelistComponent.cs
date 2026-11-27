// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Materials;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Materials;

/// <summary>
/// Makes this material storage always have a set of materials whitelisted, regardless of lathe.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MaterialStorageWhitelistComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<MaterialPrototype>> Whitelist = new();
}
