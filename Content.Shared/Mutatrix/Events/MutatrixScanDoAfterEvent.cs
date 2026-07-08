// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Mutatrix.Events;

/// <summary>
/// Completion event for a Mutatrix scan.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class MutatrixScanDoAfterEvent : DoAfterEvent
{
    /// <summary>
    /// Net entity being scanned. The normal DoAfter target also stores this, but
    /// carrying it here makes duplicate checks and debugging clearer.
    /// </summary>
    public NetEntity TargetEntity;

    /// <summary>
    /// Entity prototype ID unlocked when this scan finishes.
    /// </summary>
    public string ScannedPrototype = string.Empty;

    public MutatrixScanDoAfterEvent()
    {
    }

    public MutatrixScanDoAfterEvent(NetEntity target, string scannedPrototype)
    {
        TargetEntity = target;
        ScannedPrototype = scannedPrototype;
    }

    public override DoAfterEvent Clone()
    {
        return new MutatrixScanDoAfterEvent(TargetEntity, ScannedPrototype);
    }
}
