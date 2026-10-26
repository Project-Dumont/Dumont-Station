namespace Content.Server.Shuttles.Systems;

public sealed partial class EmergencyShuttleSystem
{
    public float? DockTimeOverride { get; private set; }

    public void SetDockTimeOverride(float seconds)
    {
        DockTimeOverride = Math.Max(0, seconds);
        if (EmergencyShuttleArrived && !ShuttlesLeft)
        {
            ConsoleAccumulator = Math.Min(ConsoleAccumulator, DockTimeOverride.Value);
            UpdateAllEmergencyConsoles();
        }
    }
}
