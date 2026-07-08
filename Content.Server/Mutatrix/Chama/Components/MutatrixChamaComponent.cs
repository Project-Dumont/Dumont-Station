// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Server.Mutatrix.Chama.Components;

/// <summary>
/// Server-side controller for Mutatrix Chama powers.
/// Keeps Chama burning and grants two fire actions without using humanoid species sprites.
/// </summary>
[RegisterComponent]
public sealed partial class MutatrixChamaComponent : Component
{
    [DataField]
    public EntProtoId FireballAction = "ActionMutatrixChamaFireball";

    [DataField]
    public EntProtoId FlameAction = "ActionMutatrixChamaFlameThrower";

    [DataField]
    public EntProtoId FireballGunProto = "MutatrixChamaFireballGun";

    [DataField]
    public EntProtoId FlameGunProto = "MutatrixChamaFlameThrowerGun";

    [DataField]
    public float FireStacks = 5f;

    public EntityUid? FireballActionEntity;
    public EntityUid? FlameActionEntity;
    public EntityUid? FireballGun;
    public EntityUid? FlameGun;

    public float FireCheckAccumulator;
}
