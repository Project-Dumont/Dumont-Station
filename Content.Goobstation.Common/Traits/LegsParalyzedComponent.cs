// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Traits;

/// <summary>
/// Set player speed to zero and standing state to down, simulating leg paralysis.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LegsParalyzedComponent : Component
{
    // Dumont start
    [DataField, AutoNetworkedField]
    public bool Permanent = true;

    [DataField, AutoNetworkedField]
    public float WalkSpeedModifier = 0.5f;

    [DataField, AutoNetworkedField]
    public float SprintSpeedModifier = 0.5f;
    // Dumont end

    [DataField] public float CrawlMoveSpeed = 2;
    [DataField] public float CrawlMoveAcceleration = 2;
};
