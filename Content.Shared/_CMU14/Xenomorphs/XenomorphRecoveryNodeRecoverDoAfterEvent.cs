// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
namespace Content.Shared._CMU14.Xenomorphs;

[Serializable, NetSerializable]
public sealed partial class XenomorphRecoveryNodeRecoverDoAfterEvent : SimpleDoAfterEvent;
