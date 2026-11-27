using System.Linq;
using Content.Server.Body.Systems;
using Content.Shared._Dumont.EntityEffects;
using Content.Shared._Shitmed.Cybernetics;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.EntityEffects;
using Content.Shared.Standing;
using Robust.Shared.Random;

namespace Content.Server._Dumont.EntityEffects;

public sealed partial class ReplaceRandomLimbSystem : EntitySystem
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<ReplaceRandomLimb>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<ReplaceRandomLimb> args)
    {
        var uid = args.Args.TargetEntity;
        if (!TryComp<BodyComponent>(uid, out var body) || args.Effect.Limbs.Count == 0)
            return;

        var limb = _random.Pick(args.Effect.Limbs);
        var coords = Transform(uid).Coordinates;
        var part = SpawnAtPosition(limb.Part, coords);
        if (!TryComp<BodyPartComponent>(part, out var partComp) || !TryReplace(uid, body, part, partComp, limb.Child))
            Del(part);

        _standing.Stand(uid);
    }

    private bool TryReplace(EntityUid uid, BodyComponent body, EntityUid part, BodyPartComponent partComp, string? child)
    {
        var old = _body.GetBodyChildrenOfType(uid, partComp.PartType, body, partComp.Symmetry).FirstOrDefault();
        if (!old.Id.Valid || HasComp<CyberneticsComponent>(old.Id) ||
            _body.GetParentPartAndSlotOrNull(old.Id) is not { } parent)
            return false;

        var childSlots = old.Component.Children.Values.ToList();
        if (!_body.DetachPart(parent.Parent, parent.Slot, old.Id))
            return false;

        if (!_body.AttachPart(parent.Parent, parent.Slot, part))
        {
            _body.AttachPart(parent.Parent, parent.Slot, old.Id);
            return false;
        }

        Del(old.Id);

        if (child == null || childSlots.Count == 0)
            return true;

        var slot = childSlots[0];
        var childPart = SpawnAtPosition(child, Transform(uid).Coordinates);
        if (!_body.TryCreatePartSlotAndAttach(part, slot.Id, childPart, slot.Type, slot.Symmetry))
            Del(childPart);

        return true;
    }
}
