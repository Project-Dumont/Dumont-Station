// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._CorvaxGoob.Photo;

[RegisterComponent]
public sealed partial class PhotoCameraUserComponent : Component
{
    [DataField]
    public string AlertPrototype = "PhotoCameraUsed";
}
