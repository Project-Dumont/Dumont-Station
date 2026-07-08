// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Mutatrix.QuatroBracos.Components;

[RegisterComponent]
public sealed partial class MutatrixQuatroBracosComponent : Component
{
    [DataField]
    public EntProtoId KickAction = "ActionMutatrixQuatroBracosKick";

    public EntityUid? KickActionEntity;

    [DataField]
    public float BluntDamage = 26f;

    [DataField]
    public float StructuralDamage = 120f;

    [DataField]
    public float KnockbackImpulse = 1000f;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2.5f);

    [DataField]
    public SoundSpecifier? KickSound = new SoundCollectionSpecifier("FootstepThud");
}
