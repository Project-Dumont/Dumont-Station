using Content.Shared.Emp;
using Content.Shared.Maps;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModEmpPulseModuleSystem : EntitySystem
{
    [Dependency] private SharedEmpSystem _emp = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    [SubscribeLocalEvent]
    private void OnUsed(Entity<ModEmpPulseModuleComponent> ent, ref ModModuleUsedEvent args)
    {
        _emp.EmpPulse(_transform.GetMapCoordinates(args.User), ent.Comp.Range, ent.Comp.EnergyConsumption, (float) ent.Comp.Duration.TotalSeconds);
        args.Handled = true;
    }
}
