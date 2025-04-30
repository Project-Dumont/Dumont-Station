using Content.Server.Forensics;
using Content.Shared.WashingMachine;

namespace Content.Server.WashingMachine;

public sealed partial class WashingMachineSystem : SharedWashingMachineSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void UpdateForensics(Entity<WashingMachineComponent> ent, HashSet<EntityUid> items)
    {
        if (!TryComp<ForensicsComponent>(ent.Owner, out var forensics))
            return;

        foreach (var item in items)
        {
            if (!TryComp<FiberComponent>(item, out var fiber))
                continue;

            var fiberText = fiber.FiberColor == null
                ? Loc.GetString("forensic-fibers", ("material", fiber.FiberMaterial))
                : Loc.GetString("forensic-fibers-colored", ("color", fiber.FiberColor), ("material", fiber.FiberMaterial));

            forensics.Fibers.Add(fiberText);
        }
    }
}
