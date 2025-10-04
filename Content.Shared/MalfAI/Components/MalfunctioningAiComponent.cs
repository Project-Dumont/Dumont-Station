// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.MalfAI.Components;

/// <summary>
/// Marker component placed on the Station AI when it becomes a Malfunctioning AI antagonist.
/// Used to gate special interactions (e.g., APC CPU siphoning) without affecting visuals like EMAG.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfunctioningAiComponent : Component
{
    [DataField]
    public LocId StoreName = "store-preset-name-malfai";

    [DataField]
    public string[] StoreCategories = { "All", "MalfAI", "Deception", "Factory", "Disruption" };

    [DataField]
    public string CurrencyId = "CPU";

    [DataField]
    public string OpenStoreAction = "ActionMalfAiOpenStore";

    [DataField]
    public string OpenBorgsUiAction = "ActionMalfAiOpenBorgsUi";

    [DataField]
    public ProtoId<AlertPrototype> CurrencyAlertId = "MalfCPU";
}
