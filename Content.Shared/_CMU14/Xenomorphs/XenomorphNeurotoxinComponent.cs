using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphNeurotoxinComponent : Component
{
    [DataField, AutoNetworkedField] public float Amount;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan BlurUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextEffect;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextInjection;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextHallucination;
}

public sealed partial class XenomorphNeurotoxinVisualSystem : EntitySystem
{
    [Dependency] private BlurryVisionSystem _blur = default!;
    [Dependency] private Robust.Shared.Timing.IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphNeurotoxinComponent, GetBlurEvent>(OnBlur);
        SubscribeLocalEvent<XenomorphNeurotoxinComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphNeurotoxinComponent, ComponentShutdown>(OnShutdown);
    }
    private void OnBlur(EntityUid uid, XenomorphNeurotoxinComponent component, GetBlurEvent args)
    {
        if (component.Running && (_timing.CurTime < component.BlurUntil || component.Amount >= 10)) args.Blur += 3;
    }
    private void OnState(Entity<XenomorphNeurotoxinComponent> ent, ref AfterAutoHandleStateEvent args) => _blur.UpdateBlurMagnitude(ent.Owner);
    private void OnShutdown(Entity<XenomorphNeurotoxinComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Amount = 0;
        ent.Comp.BlurUntil = TimeSpan.Zero;
        if (!TerminatingOrDeleted(ent)) _blur.UpdateBlurMagnitude(ent.Owner);
    }
}
