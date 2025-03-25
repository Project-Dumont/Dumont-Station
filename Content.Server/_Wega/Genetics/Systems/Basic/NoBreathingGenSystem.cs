using Content.Server.Body.Components;
using Content.Shared.Genetics;

namespace Content.Server.Genetics.System;

public sealed class NoBreathingGenSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoBreathingGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<NoBreathingGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<NoBreathingGenComponent> ent, ref ComponentInit args)
    {
        if (HasComp<RespiratorComponent>(ent))
            RemComp<RespiratorComponent>(ent);
    }

    private void OnShutdown(Entity<NoBreathingGenComponent> ent, ref ComponentShutdown args)
    {
        EnsureComp<RespiratorComponent>(ent);
    }
}
