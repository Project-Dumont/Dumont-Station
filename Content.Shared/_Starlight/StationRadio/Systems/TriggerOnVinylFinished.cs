using Content.Shared._Starlight.StationRadio.Components;
using Content.Shared._Starlight.StationRadio.Events;
using Content.Shared.Trigger.Systems;

namespace Content.Shared._Starlight.StationRadio.Systems;

public sealed partial class TriggerOnVinylFinishedSystem : EntitySystem
{
    [Dependency] private TriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnVinylFinishedComponent, VinylFinishedEvent>(OnVinylFinished);
    }

    private void OnVinylFinished(Entity<TriggerOnVinylFinishedComponent> ent, ref VinylFinishedEvent ev) => _trigger.Trigger(ent.Owner, ev.Player, ent.Comp.KeyOut);
}
