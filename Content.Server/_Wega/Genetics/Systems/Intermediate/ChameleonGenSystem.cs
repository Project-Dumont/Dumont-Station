using Content.Shared.Genetics;
using Content.Shared.Stealth.Components;

namespace Content.Server.Genetics.System;

public sealed class ChameleonGenSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChameleonGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ChameleonGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<ChameleonGenComponent> ent, ref ComponentInit args)
    {
        if (HasComp<CloakOfDarknessGenComponent>(ent))
            return;

        if (!HasComp<StealthComponent>(ent))
            EnsureComp<StealthComponent>(ent);

        if (!HasComp<StealthOnMoveComponent>(ent))
        {
            var stealth = EnsureComp<StealthOnMoveComponent>(ent);
            stealth.PassiveVisibilityRate = -0.37f;
            stealth.MovementVisibilityRate = 0.20f;
        }
    }

    private void OnShutdown(Entity<ChameleonGenComponent> ent, ref ComponentShutdown args)
    {
        if (HasComp<StealthOnMoveComponent>(ent))
            RemComp<StealthOnMoveComponent>(ent);

        if (!HasComp<CloakOfDarknessGenComponent>(ent) && HasComp<StealthComponent>(ent))
            RemComp<StealthComponent>(ent);
    }
}
