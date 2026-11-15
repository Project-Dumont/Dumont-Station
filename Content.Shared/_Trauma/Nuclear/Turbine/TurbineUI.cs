// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Trauma.Nuclear.Turbine;

[Serializable, NetSerializable]
public enum TurbineUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class TurbineChangeFlowRateMessage(float flowRate) : NuclearMachineBUIMessage
{
    public float FlowRate { get; } = flowRate;
}

[Serializable, NetSerializable]
public sealed class TurbineChangeStatorLoadMessage(float statorLoad) : NuclearMachineBUIMessage
{
    public float StatorLoad { get; } = statorLoad;
}
