// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Silicons.StationAi;

/// <summary>
/// abre o monitor interno da IA.
/// </summary>
public sealed partial class ToggleAiAlertsScreenEvent : InstantActionEvent;

[Serializable, NetSerializable]
public enum AiAlertsUiKey : byte
{
    Key
}

/// <summary>
/// a ordem importa, o monitor lista do pior pro melhor
/// </summary>
[Serializable, NetSerializable]
public enum AiAlertSeverity : byte
{
    Warning,
    Danger,
}

[Serializable, NetSerializable]
public enum AiAlertKind : byte
{
    Atmos,
    Fire,
}

/// <summary>
/// uma linha viva do monitor interno da IA
/// </summary>
[Serializable, NetSerializable]
public record struct AiAlertEntry()
{
    /// <summary>
    /// o alarme. é pra onde o botão Ir leva
    /// </summary>
    public NetEntity Source = NetEntity.Invalid;

    public string Area = string.Empty;

    public AiAlertKind Kind;

    public AiAlertSeverity Severity;
}

[Serializable, NetSerializable]
public sealed class AiAlertsBuiState : BoundUserInterfaceState
{
    public List<AiAlertEntry> Alerts;

    public AiAlertsBuiState(List<AiAlertEntry> alerts)
    {
        Alerts = alerts;
    }
}

/// <summary>
/// enviada quando a IA aperta o botão Ir de uma linha
/// </summary>
[Serializable, NetSerializable]
public sealed class AiAlertWarpMessage : BoundUserInterfaceMessage
{
    public NetEntity Target;

    public AiAlertWarpMessage(NetEntity target)
    {
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class AiAlertsRefreshMessage : BoundUserInterfaceMessage;
