using Content.Server.Antag;
//using Content.Goobstation.Server.Changeling.GameTicking.Rules;
using Content.Server.Roles;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Mind.Components;
using Robust.Shared.Player;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Goobstation.Common.Changeling;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class TurnIntoLingArtifactSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TurnIntoLingArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, TurnIntoLingArtifactComponent comp, ArtifactActivatedEvent args)
    {
        if (!HasComp<MindContainerComponent>(args.Activator) || !TryComp<ActorComponent>(args.Activator, out var target))
            return;

        if (HasComp<ChangelingComponent>(args.Activator))
            return;

        var player = target.PlayerSession;

        //TODO: Arrumar isso!!
        /*if (!HasComp<SiliconComponent>(args.Activator))
            _antag.ForceMakeAntag<ChangelingRuleComponent>(player, "Changeling"); */

    }
}

