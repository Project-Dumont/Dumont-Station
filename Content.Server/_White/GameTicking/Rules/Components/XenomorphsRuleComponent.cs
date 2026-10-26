// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 loltart <159829224+loltart@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._White.Xenomorphs.Caste;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio; // Goobstation - Play music on announcement

namespace Content.Server._White.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class XenomorphsRuleComponent : Component
{
    [ViewVariables]
    public List<EntityUid> Xenomorphs = new ();

    // Dumont start
    [ViewVariables]
    public Dictionary<ProtoId<XenomorphCastePrototype>, int> TotalCastes = new();
    // Dumont end

    #region Check

    [DataField]
    public TimeSpan CheckDelay = TimeSpan.FromSeconds(30);

    [ViewVariables]
    public TimeSpan NextCheck;

    #endregion

    #region Announcement

    [DataField]
    public string? Announcement = "xenomorphs-announcement";

    [DataField] // Goobstation - play music on announcement
    public SoundSpecifier XenomorphInfestationSound =
            new SoundPathSpecifier("/Audio/_Goobstation/Music/Black_Swarm_Short.ogg")
            {
                Params = AudioParams.Default
                    .WithVolume(-16f)
            };

    [DataField]
    public Color AnnouncementColor = Color.Red;

    [DataField]
    public string? NoMoreThreatAnnouncement = "xenomorphs-no-more-threat-announcement";

    [DataField]
    public Color NoMoreThreatAnnouncementColor = Color.Gold;

    [DataField]
    public string? Sender;

    [DataField]
    public TimeSpan MinTimeToAnnouncement = TimeSpan.FromSeconds(400);

    [DataField]
    public TimeSpan MaxTimeToAnnouncement = TimeSpan.FromSeconds(450);

    [ViewVariables]
    public bool Announced;

    [ViewVariables]
    public TimeSpan? AnnouncementTime;

    #endregion

    #region RoundEnd

    [DataField]
    public WinType WinType = WinType.Neutral;

    [DataField]
    public List<WinCondition> WinConditions = new ();

    #endregion
}

public enum WinType : byte
{
    XenoMajor,
    XenoMinor,
    Neutral,
    CrewMinor,
    CrewMajor
}

public enum WinCondition : byte
{
    NukeExplodedOnStation,
    NukeActiveInStation,
    XenoTakeoverStation,
    XenoInfiltratedOnCentCom,
    AllReproduceXenoDead,
    AllCrewDead,
    HiveContactEstablished,
    CrewEscaped
}
