using Content.Shared.Chemistry;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics.UI;

public sealed class SharedDnaModifier
{
    public const string OccupantSlotName = "scanner-bodyContainer";
    public const string DiskSlotName = "diskSlot";
    public const string InputSlotName = "beakerSlot";
    public const string SolutionSlotName = "beaker";
}

[Serializable, NetSerializable]
public sealed class DnaModifierBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly NetEntity Console;
    public readonly UniqueIdentifiersPrototype? Unique;
    public readonly List<EnzymesPrototypeInfo>? Enzymes;
    public readonly EnzymeInfo? Enzyme;
    public readonly string? ScannerBodyInfo;
    public readonly string? ScannerBodyStatus;
    public readonly string? ScannerBodyDna;
    public readonly float ScannerBodyHealth;
    public readonly float ScannerBodyRadiation;
    public readonly bool ScannerHasBeaker;
    public readonly ContainerInfo? InputContainerInfo;
    public readonly bool ScannerInRange;
    public readonly bool HasDisk;
    public DnaModifierBoundUserInterfaceState(NetEntity console, UniqueIdentifiersPrototype? unique, List<EnzymesPrototypeInfo>? enzymes, EnzymeInfo? enzyme, string? scannerBodyInfo, string? scannerBodyStatus, string? scannerBodyDna, float scannerBodyHealth, float scannerBodyRadiation, bool scannerHasBeaker, ContainerInfo? inputContainerInfo, bool scannerInRange, bool hasDisk)
    {
        Console = console;
        Unique = unique;
        Enzymes = enzymes;
        Enzyme = enzyme;
        ScannerBodyInfo = scannerBodyInfo;
        ScannerBodyStatus = scannerBodyStatus;
        ScannerBodyDna = scannerBodyDna;
        ScannerBodyHealth = scannerBodyHealth;
        ScannerBodyRadiation = scannerBodyRadiation;
        ScannerHasBeaker = scannerHasBeaker;
        InputContainerInfo = inputContainerInfo;
        ScannerInRange = scannerInRange;
        HasDisk = hasDisk;
    }
}

[Serializable, NetSerializable]
public enum DnaModifierUiKey
{
    Key,
}

public enum DnaModifierReagentAmount
{
    U1 = 1,
    U5 = 5,
    U10 = 10,
    U25 = 25,
    U50 = 50,
    U100 = 100,
    All,
}
