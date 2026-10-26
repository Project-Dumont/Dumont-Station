using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphRageVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IGameTiming _timing = default!;
    private const string ShaderId = "DumontXenomorphRage";
    private static readonly ProtoId<ShaderPrototype> Shader = "DumontXenomorphAura";

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphRageComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<XenomorphTantrumComponent, ComponentRemove>(OnTantrumRemoved);
    }

    private void OnTantrumRemoved(Entity<XenomorphTantrumComponent> ent, ref ComponentRemove args)
    {
        if (!TerminatingOrDeleted(ent) && TryComp<SpriteComponent>(ent, out var sprite)) _sprite.RemovePostShader(sprite, "DumontXenomorphTantrum");
    }

    private void OnRemove(Entity<XenomorphRageComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent)) return;
        if (TryComp<SpriteComponent>(ent, out var sprite)) _sprite.RemovePostShader(sprite, ShaderId);
    }

    public override void FrameUpdate(float frameTime)
    {
        var tantrums = EntityQueryEnumerator<XenomorphTantrumComponent, SpriteComponent>();
        while (tantrums.MoveNext(out var uid, out var tantrum, out var sprite))
        {
            const string id = "DumontXenomorphTantrum";
            if (!_sprite.TryGetPostShader(sprite, id, out var aura))
            {
                _sprite.SetPostShader(sprite, new SpriteComponent.PostShaderArgs(id, _prototypes.Index(Shader).InstanceUnique()));
                if (!_sprite.TryGetPostShader(sprite, id, out aura)) continue;
            }
            aura.Shader.SetParameter("outline_color", Color.FromHex("#A31010"));
            aura.Shader.SetParameter("outline_width", 3f);
        }
        var query = EntityQueryEnumerator<XenomorphRageComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var rage, out var sprite))
        {
            if (rage.AuraUntil <= _timing.CurTime)
            {
                _sprite.RemovePostShader(sprite, ShaderId);
                continue;
            }
            if (!_sprite.TryGetPostShader(sprite, ShaderId, out var entry))
            {
                _sprite.SetPostShader(sprite, new SpriteComponent.PostShaderArgs(ShaderId, _prototypes.Index(Shader).InstanceUnique()));
                if (!_sprite.TryGetPostShader(sprite, ShaderId, out entry)) continue;
            }
            entry.Shader.SetParameter("outline_color", rage.AuraColor);
            entry.Shader.SetParameter("outline_width", rage.AuraWidth);
        }
    }
}
