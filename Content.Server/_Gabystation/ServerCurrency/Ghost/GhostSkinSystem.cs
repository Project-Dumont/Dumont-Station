// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Gabystation.ServerCurrency.Managers;
using Content.Server.Preferences.Managers;
using Content.Shared._Gabystation.ServerCurrency.Ghost;
using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation.ServerCurrency.Ghost;

public sealed class GhostSkinSystem : SharedGhostSkinSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly CurrencyStoreManager _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostSkinComponent, ComponentStartup>(ComponentStartup);
        SubscribeLocalEvent<GhostSkinComponent, PlayerAttachedEvent>(OnPlayerAttached);
        _store.OnUserSelectNewGhostSkin += OnNewSkin;
    }

    private void OnPlayerAttached(Entity<GhostSkinComponent> ent, ref PlayerAttachedEvent args)
        => UpdateGhost(ent.AsNullable());

    private void OnNewSkin(ProtoId<GhostSkinListingPrototype>? skin, NetUserId id)
    {
        if (!_player.TryGetSessionById(id, out var session))
            return;

        var uid = session.AttachedEntity;
        if (uid is null
            || !TryComp<GhostSkinComponent>(uid, out var comp))
            return;

        UpdateGhost((uid.Value, comp));
    }

    private void ComponentStartup(Entity<GhostSkinComponent> ent, ref ComponentStartup args)
        => UpdateGhost(ent.AsNullable());

    public void UpdateGhost(Entity<GhostSkinComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        if (!_player.TryGetSessionByEntity(ent.Owner, out var player))
            return;

        var maybePreference = _prefs.GetPreferencesOrNull(player.UserId);

        if (maybePreference is not { } preference)
            return;

        ent.Comp.Skin = preference.GhostSkin;
        Dirty(ent);
    }
}
