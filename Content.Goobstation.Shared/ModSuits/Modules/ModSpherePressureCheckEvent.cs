namespace Content.Goobstation.Shared.ModSuits.Modules;

[ByRefEvent]
public record struct ModSpherePressureCheckEvent(EntityUid User, float MaxPressure, bool ExceedsLimit = false);
