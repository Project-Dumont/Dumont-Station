using Content.Server.Chat.Systems;
using Content.Shared.Standing;
using Robust.Shared.Random;

namespace Content.Server.Traits.Assorted;

public sealed class UncontrollableCoughSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<UncontrollableCoughComponent, ComponentStartup>(SetupSnough);
    }

    private void SetupSnough(EntityUid uid, UncontrollableCoughComponent component, ComponentStartup args)
    {
        component.NextIncidentTime =
            _random.NextFloat(component.TimeBetweenIncidents.X, component.TimeBetweenIncidents.Y);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<UncontrollableCoughComponent>();
        while (query.MoveNext(out var ent, out var snough))
        {
            snough.NextIncidentTime -= frameTime;

            if (snough.NextIncidentTime >= 0)
                continue;

            snough.NextIncidentTime +=
                _random.NextFloat(snough.TimeBetweenIncidents.X, snough.TimeBetweenIncidents.Y);

            var dropEvent = new DropHandItemsEvent();
            RaiseLocalEvent(ent, ref dropEvent);
            _chat.TryEmoteWithChat(ent, snough.EmoteId);
        }
    }
}
