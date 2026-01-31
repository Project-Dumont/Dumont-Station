// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._CorvaxGoob.Photo;

[RegisterComponent]
public sealed partial class PhotoCardComponent : Component
{
    [DataField]
    public byte[]? ImageData;
}
