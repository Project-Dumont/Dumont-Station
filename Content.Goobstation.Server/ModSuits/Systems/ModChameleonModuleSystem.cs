using Content.Goobstation.Shared.ModSuits.Modules;
using Content.Server.Clothing.Systems;

namespace Content.Goobstation.Server.ModSuits.Systems;

public sealed class ModChameleonModuleSystem : EntitySystem
{
    [Dependency] private ChameleonClothingSystem _chameleon = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ModChameleonPrototypeEvent>(OnSetPrototype);
    }

    private void OnSetPrototype(ref ModChameleonPrototypeEvent args)
    {
        _chameleon.SetSelectedPrototype(args.Entity, args.PrototypeId, true);
    }
}
