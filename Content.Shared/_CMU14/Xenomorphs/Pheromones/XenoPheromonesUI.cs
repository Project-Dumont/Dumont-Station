// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._CMU14.Xenomorphs.Pheromones;

[Serializable, NetSerializable]
public enum XenoPheromonesUI
{
    Key
}

[Serializable, NetSerializable]
public sealed class XenoPheromonesChosenBuiMsg(XenoPheromones pheromones) : BoundUserInterfaceMessage
{
    public readonly XenoPheromones Pheromones = pheromones;
}

[Serializable, NetSerializable]
public sealed class XenoPheromonesHelpButtonBuiMsg() : BoundUserInterfaceMessage
{

}