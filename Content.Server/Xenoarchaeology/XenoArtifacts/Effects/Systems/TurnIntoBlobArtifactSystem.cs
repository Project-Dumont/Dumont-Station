using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Roles;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Mind.Components;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Goobstation.Common.Blob;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class TurnIntoBlobArtifactSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TurnIntoBlobArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, TurnIntoBlobArtifactComponent comp, ArtifactActivatedEvent args)
    {
        if (!HasComp<MindContainerComponent>(args.Activator) || !TryComp<ActorComponent>(args.Activator, out var target))
            return;

        var player = target.PlayerSession;

        EnsureComp<BlobCarrierComponent>((EntityUid) args.Activator).HasMind = HasComp<ActorComponent>((EntityUid) args.Activator);
    }
}
