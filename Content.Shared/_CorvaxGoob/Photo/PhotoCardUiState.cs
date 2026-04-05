// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.Photo;

[Serializable, NetSerializable]
public sealed class PhotoCardUiState : BoundUserInterfaceState
{
    public byte[]? ImageData { get; }

    public PhotoCardUiState(byte[]? imageData)
    {
        ImageData = imageData;
    }
}

[Serializable, NetSerializable]
public enum PhotoCardUiKey : byte
{
    Key
}
