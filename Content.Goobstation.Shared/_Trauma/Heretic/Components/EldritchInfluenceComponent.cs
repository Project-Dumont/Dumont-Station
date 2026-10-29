// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Dataset;
using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EldritchInfluenceComponent : Component
{
    [DataField]
    public bool Spent;

    [DataField]
    public SoundSpecifier? ExamineSound = new SoundCollectionSpecifier("bloodCrawl");

    [DataField]
    public LocId ExamineBaseMessage = "influence-base-message";

    [DataField]
    public int FontSize = 22;

    [DataField]
    public ProtoId<LocalizedDatasetPrototype> HeathenExamineMessages = "FractureHeathenExamineMessages";

    [DataField]
    public List<EntityEffect[]> PossibleExamineEffects = new();

    [DataField]
    public EntProtoId ExaminedRiftStatusEffect = "ExaminedRiftStatusEffect";

    [DataField]
    public TimeSpan ExamineDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Blacklist for mobs that can safely examine the influence.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
