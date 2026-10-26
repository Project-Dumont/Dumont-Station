using Content.Shared.Buckle.Components;
using Content.Shared.Rotation;

namespace Content.Server._Dumont.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphCentcommCorpseComponent : Component;

public sealed partial class XenomorphCentcommCorpseSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    private readonly List<EntityUid> _pending = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphCentcommCorpseComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<XenomorphCentcommCorpseComponent> ent, ref ComponentStartup args)
    {
        _pending.Add(ent.Owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Wait until all mapped buckle relationships have been initialized.
        foreach (var uid in _pending)
        {
            if (Deleted(uid) || TryComp<BuckleComponent>(uid, out var buckle) && buckle.Buckled)
                continue;

            // StandingState is already down in the map, so Down() would return before
            // restoring the appearance data that the client needs to rotate the body.
            _appearance.SetData(uid, RotationVisuals.RotationState, RotationState.Horizontal);
        }

        _pending.Clear();
    }
}
