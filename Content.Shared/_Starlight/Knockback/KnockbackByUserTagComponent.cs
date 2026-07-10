// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Starlight.Knockback;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class KnockbackByUserTagComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<TagPrototype>, KnockbackData> DoestContain = new();
}

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class KnockbackData
{
    public KnockbackData() { }

    [DataField]
    public float Knockback = 0;
    [DataField]
    public float StaminaMultiplier = 10;
}