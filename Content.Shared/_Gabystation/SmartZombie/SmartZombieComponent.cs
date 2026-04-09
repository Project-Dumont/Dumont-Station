// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Gabystation.SmartZombie;
/// <summary>
/// Anti-fun police, here's your fucking knob to make this component way less FUN!
/// </summary>
[RegisterComponent]
public sealed partial class SmartZombieComponent : Component
{

    [DataField]
    public float HealModifier = 0.1f;
    [DataField]
    public float DamageModifier = 0.5f;
}
