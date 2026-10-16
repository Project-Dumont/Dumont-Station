// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Goobstation.Common.Traits;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Traits.Assorted;
using Content.Trauma.Shared.Heretic.Curses.Components;
using Robust.Shared.Timing;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Curses;

using DnaDict = Dictionary<string, (float, HashSet<EntityUid>)>;

public abstract partial class SharedHereticCurseSystem : EntitySystem
{
    [Dependency] protected IGameTiming Timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseOfParalysisStatusEffectComponent, StatusEffectAppliedEvent>(OnParalysisApply);
        SubscribeLocalEvent<CurseOfParalysisStatusEffectComponent, StatusEffectRemovedEvent>(OnParalysisRemove);

        SubscribeLocalEvent<FragileCurseComponent, DamageModifyEvent>(OnModify);
    }

    private void OnModify(Entity<FragileCurseComponent> ent, ref DamageModifyEvent args)
    {
        if (!args.Damage.AnyPositive())
            return;

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, ent.Comp.ModifierSet);
    }

    private void OnParalysisApply(Entity<CurseOfParalysisStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (TryComp(args.Target, out LegsParalyzedComponent? paralyzed) && paralyzed.Permanent)
        {
            ent.Comp.WasParalyzed = true;
            return;
        }

        var comp = Factory.GetComponent<LegsParalyzedComponent>();
        comp.Permanent = false;
        comp.WalkSpeedModifier = 0.5f;
        comp.SprintSpeedModifier = 0.5f;
        AddComp(args.Target, comp, true);
    }

    private void OnParalysisRemove(Entity<CurseOfParalysisStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (Timing.ApplyingState || ent.Comp.WasParalyzed)
            return;

        if (TerminatingOrDeleted(args.Target))
            return;

        RemCompDeferred<LegsParalyzedComponent>(args.Target);
    }

    public virtual void CurseCrewmember(Entity<HereticCurseProviderComponent> provider,
        EntityUid rune,
        EntityUid user,
        bool popup,
        DnaDict? dnaDict = null)
    {
    }
}
