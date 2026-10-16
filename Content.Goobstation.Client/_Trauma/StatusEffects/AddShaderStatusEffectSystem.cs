// Dumont start
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
// Dumont end
// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Client.Graphics;
using Content.Shared.StatusEffectNew;
using Content.Trauma.Shared.StatusEffects;

namespace Content.Trauma.Client.StatusEffects;

public sealed partial class AddShaderStatusEffectSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    [SubscribeLocalEvent]
    private void OnApplied(Entity<AddShaderStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        var id = ent.Comp.Shader;
        var shader = ProtoMan.Index<ShaderPrototype>(id).Instance();
        var data = new SpriteComponent.PostShaderArgs(id, shader)
        {
            Before = ContentPostShaderIds.BeforeOutlines,
        };
        _sprite.SetPostShader(args.Target, data);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<AddShaderStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!Terminating(args.Target))
            _sprite.RemovePostShader(args.Target, ent.Comp.Shader);
    }
}
