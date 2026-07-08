// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Mutatrix.Besta.Components;

/// <summary>
/// Marker component for the Mutatrix Besta limited sight overlay.
/// It avoids using PermanentBlindness because that causes the blurry blind effect.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BestaLimitedVisionComponent : Component
{
    /// <summary>
    /// Visible circle radius in screen pixels at normal zoom.
    /// 70px aumenta bem o campo de visão da Besta, mantendo o resto escuro e permitindo que ThermalVision destaque entidades.
    /// </summary>
    [DataField]
    public float RadiusPixels = 70f;

    /// <summary>
    /// Inner clear area before the fade starts.
    /// </summary>
    [DataField]
    public float InnerRadiusPixels = 0f;
}
