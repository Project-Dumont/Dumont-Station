using System.Linq;
using Content.Server.Body.Systems;
using Content.Shared._Dumont.EntityEffects;
using Content.Shared._Shitmed.Cybernetics;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.EntityEffects;

namespace Content.Server._Dumont.EntityEffects;

public sealed partial class ReplaceOrganSystem : EntitySystem
{
    [Dependency] private BodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<ReplaceOrgan>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<ReplaceOrgan> args)
    {
        var uid = args.Args.TargetEntity;
        if (!TryComp<BodyComponent>(uid, out var body))
            return;

        var organ = SpawnAtPosition(args.Effect.Organ, Transform(uid).Coordinates);
        if (!TryComp<OrganComponent>(organ, out var organComp) || !TryReplace(uid, body, organ, organComp))
            Del(organ);
    }

    private bool TryReplace(EntityUid uid, BodyComponent body, EntityUid organ, OrganComponent organComp)
    {
        foreach (var part in _body.GetBodyChildren(uid, body))
        {
            var old = _body.GetPartOrgans(part.Id, part.Component)
                .FirstOrDefault(o => o.Component.SlotId == organComp.SlotId);
            if (!old.Id.Valid)
                continue;

            if (HasComp<CyberneticsComponent>(old.Id) || !_body.RemoveOrgan(old.Id, old.Component))
                return false;

            Del(old.Id);
            return _body.InsertOrgan(part.Id, organ, organComp.SlotId, part.Component, organComp);
        }

        return false;
    }
}
