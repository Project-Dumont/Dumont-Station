using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Revolutionary.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Revolutionary;

public sealed class HeadRevFactionSystem : EntitySystem
{
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    private static readonly ProtoId<NpcFactionPrototype> Faction = "Revolutionary";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadRevolutionaryComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<HeadRevolutionaryComponent> ent, ref MapInitEvent args)
    {
        _faction.AddFaction(ent.Owner, Faction);
    }
}
