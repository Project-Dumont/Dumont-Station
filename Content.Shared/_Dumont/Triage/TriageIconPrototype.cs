// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._Dumont.Triage;

/// <summary>
/// Uma prioridade de triagem. O ícone é desenhado por cima do ícone de saúde do MedHUD,
/// então ele deve ser só a moldura de 1px, com o miolo transparente.
/// </summary>
[Prototype]
public sealed partial class TriageIconPrototype : StatusIconPrototype, IInheritingPrototype
{
    /// <inheritdoc />
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<TriageIconPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc />
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    /// <summary>
    /// Nome da prioridade, mostrado na janela do aparelho de triagem.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Explicação do que a cor quer dizer, mostrada abaixo do nome.
    /// </summary>
    [DataField(required: true)]
    public LocId Description;

    /// <summary>
    /// Cor do botão na janela, para achar a prioridade sem ler.
    /// </summary>
    [DataField]
    public Color Color = Color.White;

    /// <summary>
    /// Desenha a cor como uma faixa em cima da barra de vida em vez de moldura no ícone de saúde.
    /// </summary>
    [DataField]
    public bool DrawOnHealthBar;
}
