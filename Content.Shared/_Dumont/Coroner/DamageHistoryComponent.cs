using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Dumont.Coroner;

[RegisterComponent]
public sealed partial class DamageHistoryComponent : Component
{
    [DataField]
    public List<DamageRecord> Records = new();

    [DataField]
    public int MaxRecords = 30;

    [DataField]
    public FixedPoint2 MinDamage = FixedPoint2.New(5);

    [DataField]
    public TimeSpan MergeWindow = TimeSpan.FromSeconds(5);
}

[DataDefinition]
public sealed partial class DamageRecord
{
    [DataField]
    public TimeSpan Time;

    [DataField]
    public TimeSpan LastTime;

    [DataField]
    public string Type = string.Empty;

    [DataField]
    public FixedPoint2 Amount;
}
