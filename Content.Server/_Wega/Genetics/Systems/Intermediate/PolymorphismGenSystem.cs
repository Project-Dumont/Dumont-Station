using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Genetics;
using Content.Shared.Humanoid;

namespace Content.Server.Genetics.System;

public sealed class PolymorphismGenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly DnaModifierSystem _dnaModifier = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PolymorphismGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PolymorphismGenComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<PolymorphismGenComponent, PolymorphismActionEvent>(OnPolymorphism);
        SubscribeLocalEvent<PolymorphismGenComponent, PolymorphismDoAfterEvent>(OnDoAfter);
    }

    private void OnInit(Entity<PolymorphismGenComponent> ent, ref ComponentInit args)
        => ent.Comp.PolymorphismActionEntity = _action.AddAction(ent, ent.Comp.PolymorphismAction);

    private void OnShutdown(Entity<PolymorphismGenComponent> ent, ref ComponentShutdown args)
        => _action.RemoveAction(ent.Comp.PolymorphismActionEntity);

    private void OnPolymorphism(Entity<PolymorphismGenComponent> ent, ref PolymorphismActionEvent args)
    {
        args.Handled = true;
        if (!HasComp<DnaModifierComponent>(ent) || !HasComp<HumanoidAppearanceComponent>(ent))
            return;

        if (!HasComp<DnaModifierComponent>(args.Target) || !HasComp<HumanoidAppearanceComponent>(args.Target))
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent,
            8f,
            new PolymorphismDoAfterEvent(),
            ent,
            args.Target
        )
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<PolymorphismGenComponent> ent, ref PolymorphismDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        if (!TryComp<DnaModifierComponent>(ent, out var dna))
            return;

        if (!TryComp<DnaModifierComponent>(args.Target, out var targetDna))
            return;

        _dnaModifier.TryCloneHumanoid((ent, dna), (args.Target.Value, targetDna));
    }
}

