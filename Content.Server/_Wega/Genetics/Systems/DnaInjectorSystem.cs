using Content.Shared.DoAfter;
using Content.Shared.Genetics;
using Content.Shared.Interaction;
using Robust.Server.Audio;

namespace Content.Server.Genetics.System;

public sealed partial class DnaModifierSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    private void InitializeInjector()
    {
        SubscribeLocalEvent<DnaModifierInjectorComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DnaModifierInjectorComponent, DnaInjectorDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<DnaModifierCleanRandomizeComponent, ComponentStartup>(OnCleanRandomize);
    }

    public void OnFillingInjector(EntityUid injector, UniqueIdentifiersPrototype? uniqueIdentifiers, List<EnzymesPrototypeInfo>? enzymesPrototypes)
    {
        if (!TryComp(injector, out DnaModifierInjectorComponent? comp))
            return;

        if (uniqueIdentifiers == null && enzymesPrototypes == null)
            return;

        if (uniqueIdentifiers != null)
        {
            comp.UniqueIdentifiers = uniqueIdentifiers;
        }

        if (enzymesPrototypes != null)
        {
            comp.EnzymesPrototypes = enzymesPrototypes;
        }

        Dirty(injector, comp);
    }

    private void OnAfterInteract(Entity<DnaModifierInjectorComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        var user = args.User;
        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(5f),
            new DnaInjectorDoAfterEvent(), args.Used, target: args.Target.Value, used: args.Used)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            MovementThreshold = 0.01f,
            NeedHand = false
        });

        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, DnaModifierInjectorComponent component, DnaInjectorDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !args.Used.HasValue || !args.Target.HasValue)
            return;

        TryDoInject((uid, component), args.Target.Value);

        args.Handled = true;
    }

    private bool TryDoInject(Entity<DnaModifierInjectorComponent> ent, EntityUid target)
    {
        if (ent.Comp.UniqueIdentifiers == null && ent.Comp.EnzymesPrototypes == null)
            return false;

        if (!TryComp(target, out DnaModifierComponent? dnaModifier))
            return false;

        if (ent.Comp.UniqueIdentifiers != null)
        {
            dnaModifier.UniqueIdentifiers = ent.Comp.UniqueIdentifiers;
        }

        if (ent.Comp.EnzymesPrototypes != null)
        {
            dnaModifier.EnzymesPrototypes = ent.Comp.EnzymesPrototypes;
        }

        Dirty(target, dnaModifier);
        ChangeDna(dnaModifier);

        _audio.PlayPvs(ent.Comp.InjectSound, target);

        _entManager.DeleteEntity(ent);

        return true;
    }

    /// <summary>
    /// Generating pure SE
    /// </summary>
    private void OnCleanRandomize(Entity<DnaModifierCleanRandomizeComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<DnaModifierInjectorComponent>(ent, out var injector))
            return;

        var enzymesPrototypes = _enzymesIndexer.GetAllEnzymesPrototypes();
        var uniqueEnzymesPrototypes = new List<EnzymesPrototypeInfo>();
        foreach (var enzymePrototype in enzymesPrototypes)
        {
            var uniqueEnzyme = new EnzymesPrototypeInfo
            {
                EnzymesPrototypeId = enzymePrototype.EnzymesPrototypeId,
                Order = enzymePrototype.Order,
                HexCode = enzymePrototype.Order == 55
                    ? GenerateLastHexCode()
                    : GenerateHexCode()
            };

            uniqueEnzymesPrototypes.Add(uniqueEnzyme);
        }

        RemComp<DnaModifierCleanRandomizeComponent>(ent);
        injector.EnzymesPrototypes = uniqueEnzymesPrototypes;
    }
}
