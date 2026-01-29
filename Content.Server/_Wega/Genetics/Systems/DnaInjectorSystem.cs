using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Genetics;
using Content.Shared.Interaction;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Genetics.System;

public sealed partial class DnaModifierSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    private static readonly ProtoId<DamageTypePrototype> Damage = "Poison";

    private void InitializeInjector()
    {
        SubscribeLocalEvent<DnaModifierInjectorComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DnaModifierInjectorComponent, DnaInjectorDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<DnaModifierCleanRandomizeComponent, ComponentStartup>(OnCleanRandomize);
    }

    public void OnFillingInjector(EntityUid injector, UniqueIdentifiersData? uniqueIdentifiers, List<EnzymesPrototypeInfo>? enzymesPrototypes)
    {
        if (!TryComp(injector, out DnaModifierInjectorComponent? comp))
            return;

        if (uniqueIdentifiers == null && enzymesPrototypes == null)
            return;

        comp.UniqueIdentifiers = uniqueIdentifiers != null
            ? CloneUniqueIdentifiers(uniqueIdentifiers)
            : null;

        comp.EnzymesPrototypes = enzymesPrototypes != null
            ? CloneEnzymesPrototypes(enzymesPrototypes)
            : null;

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
            if (ent.Comp.EnzymesPrototypes.Count > 1)
            {
                dnaModifier.EnzymesPrototypes = ent.Comp.EnzymesPrototypes;
            }
            else if (ent.Comp.EnzymesPrototypes.Count == 1 && dnaModifier.EnzymesPrototypes != null)
            {
                var newCode = ent.Comp.EnzymesPrototypes[0];
                var existingCode = dnaModifier.EnzymesPrototypes.FirstOrDefault(x => x.Order == newCode.Order);
                if (existingCode != null)
                {
                    existingCode.HexCode = newCode.HexCode;
                }
            }
        }

        Dirty(target, dnaModifier);
        ChangeDna(target);

        _audio.PlayPvs(ent.Comp.InjectSound, target);

        var damage = new DamageSpecifier { DamageDict = { { Damage, 5 } } };
        _damage.TryChangeDamage(target, damage, true);

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

        injector.EnzymesPrototypes = uniqueEnzymesPrototypes;
    }
}
