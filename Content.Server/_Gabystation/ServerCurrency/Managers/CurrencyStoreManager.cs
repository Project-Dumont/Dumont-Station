// SPDX-FileCopyrightText: 2026 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2026 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using System.Threading.Tasks;
using System.Linq;
using Robust.Shared.Player;
using Content.Server._Gabystation.ServerCurrency.Ghost;

namespace Content.Server._Gabystation.ServerCurrency.Managers;

public sealed class CurrencyStoreManager : IPostInjectInit
{
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;
    [Dependency] private readonly IEntitySystemManager _entity = default!;

    public event Action<ProtoId<GhostSkinListingPrototype>?, NetUserId>? OnUserSelectNewGhostSkin;
    public event Action<ProtoId<TitleListingPrototype>?, NetUserId>? OnUserSelectNewTitle;

    private ISawmill _sawmill = default!;

    public void Initialize()
    {
        //_netMan.RegisterNetMessage<MsgSelectTitle>(HandleSelectTitleMessage);
        _sawmill = _log.GetSawmill("title");
    }

    #region GhostSkin

    public bool TrySetGhostSkin(NetUserId userId, ProtoId<GhostSkinListingPrototype>? ghost)
    {
        if (ghost is not null && !_proto.HasIndex(ghost))
            return false;

        _db.SaveGhostSkinAsync(userId, ghost);
        var prefs = _prefs.GetPreferences(userId);
        prefs.GhostSkin = ghost?.Id; // save in cached prefs
        OnUserSelectNewGhostSkin?.Invoke(ghost, userId);

        return true;
    }

    public List<ProtoId<GhostSkinListingPrototype>> GetOwnedGhostSkins(NetUserId userId)
    {
        var list = Task.Run(() =>
                _db.GetStorePurchasesAsync(userId, GabyModel.DbPurchaseType.GhostSkin))
            .GetAwaiter()
            .GetResult();

        return list
            .Select(id => new ProtoId<GhostSkinListingPrototype>(id.Prototype))
            .ToList();
    }

    public bool HasGhostSkin(NetUserId userId, ProtoId<GhostSkinListingPrototype> ghostSkin)
    {
        var result = Task.Run(() =>
                _db.HasStorePurchaseAsync(
                    userId,
                    GabyModel.DbPurchaseType.GhostSkin,
                    ghostSkin.Id))
            .GetAwaiter()
            .GetResult();

        return result;
    }

    public void AddGhostSkin(NetUserId userId, ProtoId<GhostSkinListingPrototype> ghostSkin)
    {
        if (HasGhostSkin(userId, ghostSkin))
            return;

        Task.Run(() =>
            _db.AddStorePurchaseAsync(
                userId,
                GabyModel.DbPurchaseType.GhostSkin,
                ghostSkin.Id
            )
        ).GetAwaiter().GetResult();
    }

    public void RemoveGhostSkin(NetUserId userId, ProtoId<GhostSkinListingPrototype> ghostSkin)
    {
        Task.Run(() =>
            _db.RemoveStorePurchaseAsync(
                userId,
                GabyModel.DbPurchaseType.GhostSkin,
                ghostSkin.Id
            )
        ).GetAwaiter().GetResult();
    }

    #endregion

    #region titles

    public bool TrySetTitle(NetUserId userId, ProtoId<TitleListingPrototype>? title)
    {
        if (title is not null && !_proto.HasIndex(title))
            return false;

        _db.SaveOOCTitleAsync(userId, title);
        var prefs = _prefs.GetPreferences(userId);
        prefs.OOCTitle = title?.Id; // save in cached prefs
        OnUserSelectNewTitle?.Invoke(title, userId);

        return true;
    }

    public List<ProtoId<TitleListingPrototype>> GetOwnedTitles(NetUserId userId)
    {
        var list = Task.Run(() =>
                _db.GetStorePurchasesAsync(userId, GabyModel.DbPurchaseType.Title))
            .GetAwaiter()
            .GetResult();

        return list
            .Select(id => new ProtoId<TitleListingPrototype>(id.Prototype))
            .ToList();
    }

    public bool HasTitle(NetUserId userId, ProtoId<TitleListingPrototype> title)
    {
        var result = Task.Run(() =>
                _db.HasStorePurchaseAsync(userId, GabyModel.DbPurchaseType.Title, title))
            .GetAwaiter()
            .GetResult();

        return result;
    }

    public void AddTitle(NetUserId userId, ProtoId<TitleListingPrototype> title)
    {
        if (HasTitle(userId, title))
            return;

        Task.Run(() =>
            _db.AddStorePurchaseAsync(
                userId,
                GabyModel.DbPurchaseType.Title,
                title.Id
            )
        ).GetAwaiter().GetResult();
    }

    public void RemoveTitle(NetUserId userId, ProtoId<TitleListingPrototype> title)
    {
        Task.Run(() =>
            _db.RemoveStorePurchaseAsync(
                userId,
                GabyModel.DbPurchaseType.Title,
                title.Id
            )
        ).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Returns the "chat" version of a title like \[Title Name]
    /// </summary>
    public string? SanitizeTitleString(ProtoId<TitleListingPrototype>? title)
    {
        if (title is null || !_proto.TryIndex(title, out var proto))
            return "";

        var str = Loc.GetString(proto.Title);
        var sanitazed = $"\\[{str}] ";
        if (proto.Color is not null)
            sanitazed = $"\\[[color={proto.Color}]{str}[/color]] ";

        return sanitazed;
    }

    #endregion

    public void FinishLoad(ICommonSession session)
    {
        if (session.AttachedEntity is not { } entity)
            return;

        if (!_entity.TryGetEntitySystem<GhostSkinSystem>(out var ghostSkin))
            return;

        ghostSkin.UpdateGhost(entity);
    }

    public void PostInject()
    {
        _userDb.AddOnFinishLoad(FinishLoad);
    }
}
