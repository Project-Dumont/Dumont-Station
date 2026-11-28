// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Nuke;
using Robust.Shared.Audio;

namespace Content.Server._Dumont.Nuke;

/// <summary>
/// S.A.A.E. audio for a nuke: siren while it's armed, and its own announcements for rearming and disarming.
/// </summary>
[RegisterComponent, Access(typeof(NukeSystem))]
public sealed partial class NukeSaaeComponent : Component
{
    /// <summary>
    /// Loops once the S.A.A.E. is done talking, until the nuke song starts.
    /// </summary>
    [DataField]
    public SoundSpecifier Siren = new SoundPathSpecifier("/Audio/_Dumont/Machines/Nuke/nuclear_siren.ogg", AudioParams.Default.WithLoop(true).WithVolume(-10f));

    /// <summary>
    /// Replaces the alert level sound when the nuke gets armed again.
    /// </summary>
    [DataField]
    public SoundSpecifier RearmSound = new SoundPathSpecifier("/Audio/_Dumont/Announcements/nuke_reactivated_saae.ogg");

    /// <summary>
    /// Replaces the alert level sound when the nuke gets disarmed.
    /// </summary>
    [DataField]
    public SoundSpecifier DisarmSound = new SoundPathSpecifier("/Audio/_Dumont/Announcements/nuke_cancelled_saae.ogg");

    [ViewVariables]
    public bool ArmedBefore;

    [ViewVariables]
    public bool SirenPending;

    [ViewVariables]
    public EntityUid? SirenStream;
}
