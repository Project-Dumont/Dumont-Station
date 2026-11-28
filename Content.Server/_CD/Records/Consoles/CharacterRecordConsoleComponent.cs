using Content.Shared._CD.Records;
using Content.Shared._Dumont.Triage;
using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Records.Consoles;

[RegisterComponent]
public sealed partial class CharacterRecordConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public uint? SelectedIndex { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public StationRecordsFilter? Filter;

    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public RecordConsoleType ConsoleType;

    // Dumont
    /// <summary>
    /// Prioridades de triagem que este console permite escolher (só nos consoles médico e admin).
    /// </summary>
    [DataField]
    public List<ProtoId<TriageIconPrototype>> TriageLevels = new()
    {
        "TriageIconImmediate",
        "TriageIconVeryUrgent",
        "TriageIconUrgent",
        "TriageIconStandard",
        "TriageIconNonUrgent",
        "TriageIconDeceased",
    };
}
