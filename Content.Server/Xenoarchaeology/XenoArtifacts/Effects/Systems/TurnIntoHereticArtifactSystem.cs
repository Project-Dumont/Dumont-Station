using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Roles;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Mind.Components;
using Robust.Shared.Player;
using Content.Server.Roles;
using Content.Shared.Heretic;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class TurnIntoHereticArtifactSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TurnIntoHereticArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, TurnIntoHereticArtifactComponent comp, ArtifactActivatedEvent args)
    {
        if (!HasComp<MindContainerComponent>(args.Activator) || !TryComp<ActorComponent>(args.Activator, out var target))
            return;

        var player = target.PlayerSession;

        if(HasComp<HereticComponent>(args.Activator))
            return;

        _antag.ForceMakeAntag<HereticRuleComponent>(player, "Heretic");
    }
}

