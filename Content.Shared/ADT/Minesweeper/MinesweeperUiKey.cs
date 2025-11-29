// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Minesweeper;

[Serializable, NetSerializable]
public enum MinesweeperUiKey : byte
{
    Key,
}
