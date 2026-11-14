using Content.Shared._Dumont.Triage;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Records;

[Serializable, NetSerializable]
public enum CharacterRecordConsoleKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum RecordConsoleType : byte
{
    Security,
    Medical,
    Employment,
    Generic,
    /// <summary>
    /// Admin console has the functionality of all other types and has some additional admin related functionality
    /// </summary>
    Admin
}

[Serializable, NetSerializable]
public sealed class CharacterRecordConsoleState : BoundUserInterfaceState
{
    [Serializable, NetSerializable]
    public struct CharacterInfo
    {
        public string CharacterDisplayName;
        public uint? StationRecordKey;
    }

    public RecordConsoleType ConsoleType { get; set; }

    /// <summary>
    /// Character selected in the console
    /// </summary>
    public uint? SelectedIndex { get; set; } = null;

    /// <summary>
    /// List of names+station record keys to display in the listing
    /// </summary>
    public Dictionary<uint, CharacterInfo>? CharacterList { get; set; }

    /// <summary>
    /// The contents of the selected record
    /// </summary>
    public FullCharacterRecords? SelectedRecord { get; set; } = null;

    public StationRecordsFilter? Filter { get; set; } = null;

    /// <summary>
    /// Security status of the selected record
    /// </summary>
    public (SecurityStatus, string?)? SelectedSecurityStatus = null;

    // Dumont
    /// <summary>
    /// Prioridades de triagem que o console oferece. Nulo quando o console não mexe com triagem.
    /// </summary>
    public List<ProtoId<TriageIconPrototype>>? TriageLevels = null;

    /// <summary>
    /// Triagem atual do paciente do registro selecionado. Nulo se não tem marcação.
    /// </summary>
    public ProtoId<TriageIconPrototype>? SelectedTriage = null;

    /// <summary>
    /// Se dá para mudar a triagem do registro selecionado (o paciente ainda existe).
    /// </summary>
    public bool CanSetTriage = false;
}

// Dumont
[Serializable, NetSerializable]
public sealed class CharacterRecordSetTriageMsg : BoundUserInterfaceMessage
{
    /// <summary>
    /// Nova prioridade, ou nulo para tirar a marcação.
    /// </summary>
    public readonly ProtoId<TriageIconPrototype>? Level;

    public CharacterRecordSetTriageMsg(ProtoId<TriageIconPrototype>? level)
    {
        Level = level;
    }
}

[Serializable, NetSerializable]
public sealed class CharacterRecordsConsoleFilterMsg : BoundUserInterfaceMessage
{
    public readonly StationRecordsFilter? Filter;

    public CharacterRecordsConsoleFilterMsg(StationRecordsFilter? filter)
    {
        Filter = filter;
    }
}

[Serializable, NetSerializable]
public sealed class CharacterRecordConsoleSelectMsg : BoundUserInterfaceMessage
{
    public readonly uint? CharacterRecordKey;

    public CharacterRecordConsoleSelectMsg(uint? characterRecordKey)
    {
        CharacterRecordKey = characterRecordKey;
    }
}
