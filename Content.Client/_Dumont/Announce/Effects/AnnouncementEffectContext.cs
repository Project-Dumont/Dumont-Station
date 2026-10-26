// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Announce;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Dumont.Announce.Effects;

public readonly record struct AnnouncementEffectContext(
    AnnouncementStyle Style,
    ActiveAnnouncement State,
    IReadOnlyList<RichTextLabel> Labels,
    bool HasTitle);
