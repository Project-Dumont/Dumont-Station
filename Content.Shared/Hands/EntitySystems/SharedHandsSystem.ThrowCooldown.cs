using Content.Shared.Hands.Components;

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem
{
    public void SetNextThrowTime(Entity<HandsComponent> ent, TimeSpan nextThrowTime)
    {
        ent.Comp.NextThrowTime = nextThrowTime;
        Dirty(ent);
    }
}
