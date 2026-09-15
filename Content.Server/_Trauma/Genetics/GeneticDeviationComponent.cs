// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Shared.Genetics.Mutations;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Server.Genetics;

[RegisterComponent]
public sealed partial class GeneticDeviationComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId<MutationComponent>> Mutations = new();

    /// <summary>
    /// Quantas sortear, contando as duas pontas.
    /// </summary>
    [DataField]
    public int Min = 1;

    /// <inheritdoc cref="Min"/>
    [DataField]
    public int Max = 2;

    public bool Sorteado;
}
