// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Language;
using Content.Shared._White.Xenomorphs.Caste;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._White.Xenomorphs.Xenomorph;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphComponent : Component
{
    [DataField(required: true)]
    public ProtoId<XenomorphCastePrototype> Caste;

    /// <summary>
    /// Healing provided by the weed.
    /// </summary>
    [DataField]
    public DamageSpecifier? WeedHeal;

    [DataField]
    public TimeSpan WeedHealRate = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Language on which xenomorph need to speak to send hivemind message.
    /// </summary>
    [DataField]
    public ProtoId<LanguagePrototype> XenoLanguageId { get; set; } = "XenoHivemind";

    [ViewVariables]
    public bool OnWeed;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan WeedHealBlockedUntil;

    [ViewVariables]
    public TimeSpan NextPointsAt;
}
