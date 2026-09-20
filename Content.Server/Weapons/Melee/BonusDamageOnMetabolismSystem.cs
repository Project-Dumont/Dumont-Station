// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Body.Components;
using Content.Shared.Body.Prototypes;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Melee.Metabolizer;
using Robust.Shared.Prototypes;

namespace Content.Server.Weapons.Melee;

public sealed class BonusDamageOnMetabolismSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BonusDamageOnMetabolismComponent, GetVerbsEvent<Verb>>(OnGetVerb);
        SubscribeLocalEvent<BonusDamageOnMetabolismComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnGetVerb(Entity<BonusDamageOnMetabolismComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var metabolizers = _proto.EnumeratePrototypes<MetabolizerTypePrototype>()
            .OrderBy(x => Loc.GetString(x.LocalizedName));

        byte index = 0;
        foreach (var metabolizer in metabolizers)
        {
            if (ent.Comp.ExcludedMetabolizers.Contains(metabolizer.ID))
                continue;

            var id = metabolizer.ID;
            args.Verbs.Add(new Verb
            {
                Priority = index--,
                Category = VerbCategory.SelectType,
                Text = Loc.GetString(metabolizer.LocalizedName),
                Disabled = ent.Comp.SelectedMetabolizer == id,
                Act = () =>
                {
                    ent.Comp.SelectedMetabolizer = id;
                    Dirty(ent);
                },
            });
        }
    }

    private void OnMeleeHit(Entity<BonusDamageOnMetabolismComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.SelectedMetabolizer is not { } selected)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (TryComp<MobStateComponent>(hit, out var mobState) && !ent.Comp.ValidMobStates.Contains(mobState.CurrentState))
                continue;

            if (!HasMetabolizer(hit, selected))
                continue;

            args.BonusDamage += ent.Comp.Damage;
            return;
        }
    }

    private bool HasMetabolizer(EntityUid uid, ProtoId<MetabolizerTypePrototype> type)
    {
        foreach (var organ in _body.GetBodyOrganEntityComps<MetabolizerComponent>(uid))
        {
            if (organ.Comp1.MetabolizerTypes?.Contains(type) == true)
                return true;
        }

        return false;
    }
}
