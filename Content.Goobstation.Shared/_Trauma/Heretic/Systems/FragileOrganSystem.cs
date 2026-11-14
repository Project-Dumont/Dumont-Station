// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Medical.Shared.Body;
using System.Linq;
using Content.Shared.Body.Events;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Gibbing.Events;
using Content.Shared.Gibbing.Systems;
using Robust.Shared.Network;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems;

public sealed class FragileOrganSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly INetManager _net = default!;

    private readonly HashSet<EntityUid> _removed = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FragileOrganComponent, OrganRemovedEvent>(OnOrganRemoved);
        SubscribeLocalEvent<FragileOrganComponent, BodyPartRemovedEvent>(OnPartRemoved);
    }

    private void OnOrganRemoved(Entity<FragileOrganComponent> ent, ref OrganRemovedEvent args)
    {
        if (_net.IsServer)
            _removed.Add(ent);
    }

    private void OnPartRemoved(Entity<FragileOrganComponent> ent, ref BodyPartRemovedEvent args)
    {
        if (_net.IsServer && args.Part.Owner == ent.Owner)
            _removed.Add(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var removed = _removed.ToArray();
        _removed.Clear();
        foreach (var uid in removed)
        {
            if (TerminatingOrDeleted(uid))
                continue;
            if (TryComp<BodyPartComponent>(uid, out var part))
                _body.GibPart(uid, part);
            else
                _gibbing.TryGibEntity(uid, uid, GibType.Gib, GibContentsOption.Drop, out _);
            QueueDel(uid);
        }
    }
}
// Dumont end
