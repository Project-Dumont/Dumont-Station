// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Mindshield.Components;

namespace Content.Trauma.Server.Revolutionary;

/// <summary>
/// Handles removing a real mindshield and replacing it with a fake one for antags who start with a real mindshield
/// </summary>
public sealed class MindshieldRemovingAntagSystem : EntitySystem
{
    [Dependency] private readonly SharedSubdermalImplantSystem _subdermal = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindshieldRemovingAntagComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);
    }

    private void OnAntagSelected(Entity<MindshieldRemovingAntagComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var uid = args.EntityUid;

        if (HasComp<FakeMindShieldComponent>(uid))
            return;

        if (!TryComp<ImplantedComponent>(uid, out var implanted))
            return;

        // Dumont changes start
        EntityUid? shield = null;
        foreach (var implant in implanted.ImplantContainer.ContainedEntities)
        {
            if (!HasComp<MindShieldImplantComponent>(implant))
                continue;

            shield = implant;
            break;
        }

        if (shield is not { } found)
            return;

        _subdermal.ForceRemove(uid, found);
        // Dumont end

        _subdermal.AddImplant(uid, ent.Comp.FakeMindShieldImplant);

        if (TryComp<FakeMindShieldComponent>(uid, out var fakeMindShield))
        {
            fakeMindShield.IsEnabled = true;
            Dirty(uid, fakeMindShield);
        }
    }
}
