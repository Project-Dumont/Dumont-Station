// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Projectiles;
using Content.Shared.Trigger.Components.Triggers;

namespace Content.Shared.Trigger.Systems;

public sealed partial class TriggerOnEmbedSystem : EntitySystem
{
    [Dependency] private TriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TriggerOnEmbedComponent, EmbedEvent>(OnEmbed);
    }

    private void OnEmbed(Entity<TriggerOnEmbedComponent> ent, ref EmbedEvent args)
    {
        var user = ent.Comp.UserIsEmbeddedInto ? args.Embedded : args.Shooter;
        _trigger.Trigger(ent, user, ent.Comp.KeyOut);
    }
}
