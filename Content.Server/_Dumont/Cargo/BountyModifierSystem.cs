using Content.Server.Cargo.Components;
using Content.Trauma.Common.Cargo;
using Content.Trauma.Shared.Cargo;

namespace Content.Server._Dumont.Cargo;
public sealed partial class BountyModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BountyModifierComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<BountyModifierComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<StationCargoBountyDatabaseComponent>(ent, out var db))
            return;

        for (var i = 0; i < db.Bounties.Count; i++)
        {
            var bounty = db.Bounties[i];
            var ev = new ModifyBountyRewardEvent(ProtoMan.Index(bounty.Bounty).Reward);
            RaiseLocalEvent(ent, ref ev);
            db.Bounties[i] = bounty with { Reward = ev.Reward };
        }
    }
}
