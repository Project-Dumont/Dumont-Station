using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Dumont.Coroner;

public abstract class SharedCoronerSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AutopsyToolComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<AutopsyToolComponent, AutopsyDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<AutopsyToolComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (!HasComp<MobStateComponent>(target) || !_mobState.IsDead(target))
            return;

        args.Handled = true;

        if (!HasComp<AutopsyUserComponent>(args.User))
        {
            _popup.PopupClient(Loc.GetString("autopsy-untrained"), args.User, args.User);
            return;
        }

        var duration = OnTable(target) ? ent.Comp.TableDuration : ent.Comp.Duration;

        var doAfter = new DoAfterArgs(EntityManager, args.User, duration, new AutopsyDoAfterEvent(), ent, target, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameTool,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        _audio.PlayPredicted(ent.Comp.Sound, target, args.User);
        _popup.PopupClient(Loc.GetString("autopsy-start", ("target", Identity.Entity(target, EntityManager))), args.User, args.User);
    }

    private void OnDoAfter(Entity<AutopsyToolComponent> ent, ref AutopsyDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target || !_mobState.IsDead(target))
            return;

        args.Handled = true;
        Autopsy(ent, args.User, target, OnTable(target));
    }

    protected bool OnTable(EntityUid target)
    {
        return TryComp<BuckleComponent>(target, out var buckle)
               && buckle.BuckledTo is { } strap
               && HasComp<AutopsyTableComponent>(strap);
    }

    protected virtual void Autopsy(Entity<AutopsyToolComponent> ent, EntityUid user, EntityUid target, bool onTable)
    {
    }
}
