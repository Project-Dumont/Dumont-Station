// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.RoundEnd;

[Serializable, NetSerializable]
public sealed class PhotoAlbumEvent : EntityEventArgs
{
    public List<AlbumData>? Albums { get; }

    public PhotoAlbumEvent(List<AlbumData>? albums)
    {
        Albums = albums;
    }
}

[Serializable, NetSerializable]
public struct AlbumData
{
    public Dictionary<byte[], string?> Images;

    public string? AuthorCkey;
    public string? AuthorName;

    public AlbumData(Dictionary<byte[], string?> images, string? authorCkey, string? authorName)
    {
        this.Images = images;
        this.AuthorCkey = authorCkey;
        this.AuthorName = authorName;
    }
}
