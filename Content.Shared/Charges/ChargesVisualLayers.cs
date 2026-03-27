// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// TraumaStation addition

using Robust.Shared.Serialization;

namespace Content.Shared.Charges;

[Serializable, NetSerializable]
public enum ChargesVisuals : byte
{
    Charges
}

[Serializable, NetSerializable]
public enum ChargesVisualLayers : byte
{
    ChargesLeft
}

