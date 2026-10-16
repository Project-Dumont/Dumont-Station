// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.MalfAi.Events;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class RadioKeyUnlockedEvent : EntityEventArgs
{
    [DataField(required: true)]
    public List<string> Channels = new();

    public RadioKeyUnlockedEvent(List<string> channels)
    {
        Channels = channels;
    }
};