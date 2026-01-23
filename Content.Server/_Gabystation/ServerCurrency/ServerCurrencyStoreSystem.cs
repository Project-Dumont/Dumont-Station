// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Shared._Gabystation.CCVar;
using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation.ServerCurrency;

public sealed class ServerCurrencyStoreSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    public List<TokenListingPrototype> RotationStorage = [];
    private readonly ProtoId<WeightedRandomPrototype> _storeListProto = "CurrencyStoreRotation";

    public event Action? NewRotation;

    public float RotationCooldown = 999f;
    private float _rotationCooldownTime = 999f;
    private int _maxTokenPerRotation = 3;

    public override void Initialize()
    {
        Subs.CVar(_cfg, GabyCVars.SCurrencyRotationCooldown, value => _rotationCooldownTime = value, true);
        Subs.CVar(_cfg, GabyCVars.SCurrencyStoreTokens, value => _maxTokenPerRotation = value, true);
        DoStoreRotation();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (RotationCooldown >= 0f)
        {
            RotationCooldown -= frameTime;
            return;
        }

        DoStoreRotation();
    }

    public void DoStoreRotation()
    {
        Log.Debug("Doing new store rotation...");
        RotationCooldown = _rotationCooldownTime;
        RotationStorage.Clear();

        if (!_prototype.TryIndex(_storeListProto, out var listProto))
            return;

        var picked = new HashSet<ProtoId<TokenListingPrototype>>();
        var safety = 0;

        while (RotationStorage.Count < _maxTokenPerRotation && safety < 50)
        {
            safety++;

            var proto = listProto.Pick();

            if (!picked.Add(proto))
                continue;

            if (!_prototype.TryIndex<TokenListingPrototype>(proto, out var token))
                continue;

            RotationStorage.Add(token);
            Log.Debug($"[TOKEN STORE] Added {proto} to store.");
            NewRotation?.Invoke();
        }

        _chatManager.DispatchServerAnnouncement(Loc.GetString("gs-balanceui-shop-rotation-announce"));
    }
}
