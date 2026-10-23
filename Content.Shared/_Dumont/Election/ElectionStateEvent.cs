using Content.Shared.Preferences;
using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Election;

[Serializable, NetSerializable]
public enum ElectionPhase : byte
{
    Standby,
    Waiting,
    Counting,
    SecondRound,
    Elected,
}

[Serializable, NetSerializable]
public enum ElectionOutcome : byte
{
    None,
    SecondRound,
    Elected,
}

[Serializable, NetSerializable]
public sealed class ElectionCandidate
{
    public string Name = string.Empty;
    public string Party = string.Empty;
    public HumanoidCharacterProfile Profile = default!;
    public string? Job;
    public long Votes;
    public float Percent;
    public ElectionOutcome Outcome;
}

/// <summary>
/// everything the screens draw. only personas go over the wire, never the real names
/// </summary>
[Serializable, NetSerializable]
public sealed class ElectionStateEvent : EntityEventArgs
{
    public ElectionPhase Phase;
    public TimeSpan StartsAt;
    public List<ElectionCandidate> Candidates = new();
    public float Sections;
    public string LastUpdate = string.Empty;
}
