using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Clothing.Components;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModChameleonModuleSystem : EntitySystem
{
    [Dependency] private ModSuitSystem _modSuit = default!;

    [SubscribeLocalEvent]
    private void OnInstalled(Entity<ModChameleonModuleComponent> ent, ref ModModuleInstalledEvent args)
    {
        if (HasComp<ChameleonClothingComponent>(args.Suit) || MetaData(args.Suit).EntityPrototype is not { } proto)
            return;

        EntityManager.AddComponents(args.Suit, ent.Comp.Components);
        ent.Comp.Granted = true;
        var ev = new ModChameleonPrototypeEvent(args.Suit, proto.ID);
        RaiseLocalEvent(ref ev);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<ModChameleonModuleComponent> ent, ref ModModuleRemovedEvent args)
    {
        if (!ent.Comp.Granted)
            return;

        ent.Comp.Granted = false;
        if (TerminatingOrDeleted(args.Suit))
            return;

        if (MetaData(args.Suit).EntityPrototype is { } proto)
        {
            var ev = new ModChameleonPrototypeEvent(args.Suit, proto.ID);
            RaiseLocalEvent(ref ev);
        }

        _modSuit.RevokeComponents(args.Suit, ent.Comp.Components);
    }
}
