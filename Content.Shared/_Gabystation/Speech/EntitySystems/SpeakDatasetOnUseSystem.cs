using Content.Shared.Chat;
using Content.Shared._Gabystation.Speech.Components;
using Content.Shared.Dataset;
using Content.Shared.Interaction.Events;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Gabystation.Speech.EntitySystems;

public sealed class SpeakDatasetOnUseSystem : EntitySystem
{
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeakDatasetOnUseComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<SpeakDatasetOnUseComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || !_prototypeManager.TryIndex(ent.Comp.LocalizedDataset, out LocalizedDatasetPrototype? speechLocalization) || speechLocalization.Values.Count == 0)
            return;

        _chat.TrySendInGameICMessage(args.User, Loc.GetString(_random.Pick(speechLocalization)), InGameICChatType.Speak, hideChat: false, hideLog: false);
        args.Handled = true;
    }
}
