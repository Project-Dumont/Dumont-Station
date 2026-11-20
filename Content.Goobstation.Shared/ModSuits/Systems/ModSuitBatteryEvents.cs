namespace Content.Goobstation.Shared.ModSuits.Systems;

[ByRefEvent]
public struct ModSuitBatteryInfoEvent
{
    public bool Found;
    public float Charge;
    public float MaxCharge;
}

[ByRefEvent]
public struct ModSuitBatteryUseEvent
{
    public float Amount;
    public bool Succeeded;
}

[ByRefEvent]
public struct ModSuitBatteryChangeEvent
{
    public float Amount;
}
