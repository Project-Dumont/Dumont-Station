using Content.Shared._Orion.Power.Components;
using Content.Shared.Examine;

namespace Content.Shared._Orion.Power.Systems;

public sealed class SharedInducerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InducerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, InducerComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("inducer-examine-rate", ("rate", component.TransferRate)));
    }
}
