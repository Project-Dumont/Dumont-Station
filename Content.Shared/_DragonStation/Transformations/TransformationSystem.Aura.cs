namespace Content.Shared._DragonStation.Transformations;

public sealed partial class TransformationSystem
{
    private void EnableAura(EntityUid uid, TransformationComponent component)
    {
        if (!component.GrantsAura)
            return;

        EnsureComp<SuperSaiyanAuraComponent>(uid);
    }

    private void DisableAura(EntityUid uid)
    {
        RemCompDeferred<SuperSaiyanAuraComponent>(uid);
    }
}
