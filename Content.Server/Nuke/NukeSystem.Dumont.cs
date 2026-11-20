// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Dumont.Nuke;
using Content.Server._Dumont.Saae;
using Content.Shared.Nuke;
using Robust.Shared.Player;

namespace Content.Server.Nuke;

public sealed partial class NukeSystem
{
    [Dependency] private SaaeAnnouncerSystem _saae = default!;

    private void InitializeSaae()
    {
        SubscribeLocalEvent<NukeSaaeComponent, ComponentShutdown>(OnSaaeShutdown);
    }

    private void OnSaaeShutdown(Entity<NukeSaaeComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.SirenStream = _audio.Stop(ent.Comp.SirenStream);
    }

    private void UpdateSaae()
    {
        if (!_saae.Idle)
            return;

        var query = EntityQueryEnumerator<NukeSaaeComponent, NukeComponent>();
        while (query.MoveNext(out var uid, out var saae, out var nuke))
        {
            if (!saae.SirenPending)
                continue;

            saae.SirenPending = false;
            if (nuke.Status == NukeStatus.ARMED)
                saae.SirenStream = _audio.PlayGlobal(saae.Siren, GetSaaeFilter(uid), true)?.Entity;
        }
    }

    /// <summary>
    /// Returns true if the S.A.A.E. plays its own audio instead of the alert level sound.
    /// </summary>
    private bool SaaeArmed(EntityUid uid)
    {
        if (!TryComp<NukeSaaeComponent>(uid, out var saae))
            return false;

        var rearmed = saae.ArmedBefore;
        if (rearmed)
            _saae.Play(saae.RearmSound, GetSaaeFilter(uid), key: "alert");

        saae.ArmedBefore = true;
        saae.SirenPending = true;
        return rearmed;
    }

    /// <summary>
    /// Returns true if the S.A.A.E. plays its own audio instead of the alert level sound.
    /// </summary>
    private bool SaaeDisarmed(EntityUid uid)
    {
        if (!TryComp<NukeSaaeComponent>(uid, out var saae))
            return false;

        StopSaaeSiren((uid, saae));
        _saae.Play(saae.DisarmSound, GetSaaeFilter(uid), key: "alert");
        return true;
    }

    private void StopSaaeSiren(Entity<NukeSaaeComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.SirenPending = false;
        ent.Comp.SirenStream = _audio.Stop(ent.Comp.SirenStream);
    }

    private Filter GetSaaeFilter(EntityUid uid)
    {
        var filter = _station.GetInOwningStation(uid);
        filter.AddPlayersByPvs(uid, entityManager: EntityManager);
        return filter;
    }
}
