using Content.Shared.Weapons.Reflect;

namespace Content.Shared._DragonStation.Transformations;

public sealed partial class TransformationSystem
{
    // Borrow the same reflect component used by Sleeping Carp while the form is active.
    private void EnableReflection(EntityUid uid, TransformationComponent component)
    {
        var reflect = EnsureComp<ReflectComponent>(uid);
        reflect.Examinable = false;
        reflect.ReflectProb = component.ReflectProbability;
        reflect.Spread = component.ReflectSpread;
        Dirty(uid, reflect);
    }

    // Remove the temporary reflection granted by the form.
    private void DisableReflection(EntityUid uid)
    {
        RemComp<ReflectComponent>(uid);
    }
}
