// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Disease.Systems;
using Content.Server._Shitmed.DelayedDeath;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Trauma.Shared.Disease;

namespace Content.Trauma.Server.Disease;

public sealed partial class DiseaseHeartAttackSystem : EntitySystem
{
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private SharedDiseaseSystem _disease = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseHeartAttackEffectComponent, DiseaseEffectEvent>(OnEffect);
    }

    private void OnEffect(Entity<DiseaseHeartAttackEffectComponent> ent, ref DiseaseEffectEvent args)
    {
        var hearts = _body.GetBodyOrganEntityComps<HeartComponent>(args.Ent.Owner);
        if (hearts.Count == 0)
            return;

        foreach (var heart in hearts)
        {
            _body.RemoveOrgan(heart);
            QueueDel(heart);
        }

        var death = EnsureComp<DelayedDeathComponent>(args.Ent);
        death.DeathTime = Math.Min(death.DeathTime, death.DeathTimer + 10);
        _popup.PopupEntity(Loc.GetString("foxdie-heart-stops"), args.Ent, args.Ent, PopupType.LargeCaution);
        _disease.TryRemoveEffect(args.Disease.Owner, ent);
    }
}
