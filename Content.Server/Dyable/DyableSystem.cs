using Content.Server.Forensics;
using Content.Shared.Dyable;

namespace Content.Server.Dyable;

public sealed partial class DyableSystem : SharedDyableSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void UpdateForensics(Entity<DyableComponent> ent)
    {
        if (TryComp<FiberComponent>(ent.Owner, out var fiber))
            fiber.FiberColor = ent.Comp.Color;

        if (TryComp<ResidueComponent>(ent.Owner, out var residue))
            residue.ResidueColor = ent.Comp.Color;
    }
}
