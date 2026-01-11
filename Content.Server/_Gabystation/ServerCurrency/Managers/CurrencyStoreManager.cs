using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared._Gabystation.ServerCurrency.Titles;
using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Asynchronous;
using System.Threading.Tasks;
using System.Linq;

namespace Content.Server._Gabystation.ServerCurrency.Managers;

public sealed class CurrencyStoreManager : IPostInjectInit
{
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IServerNetManager _netMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ITaskManager _task = default!;

    private ISawmill _sawmill = default!;

    public void Initialize()
    {
        _netMan.RegisterNetMessage<MsgSelectTitle>(HandleSelectTitleMessage);
        _sawmill = _log.GetSawmill("title");
    }

    public void PostInject()
    {

    }

    public void HandleSelectTitleMessage(MsgSelectTitle msg)
    {
        TrySetTitle(msg.MsgChannel.UserId, msg.Proto);
    }

    public bool TrySetTitle(NetUserId userId, ProtoId<TitleListingPrototype>? title)
    {
        if (title is not null && !_proto.HasIndex<TitleListingPrototype>(title))
            return false;

        _db.SaveOOCTitleAsync(userId, title);
        var prefs = _prefs.GetPreferences(userId);
        prefs.OOCTitle = title?.Id; // save in cached prefs
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
    /// Returns the "chat" version of the title like \[Title Name]
    /// </summary>
    public string? SanitizeTitleString(ProtoId<TitleListingPrototype>? title)
    {
        if (title is null || !_proto.TryIndex<TitleListingPrototype>(title, out var proto))
            return "";

        var str = Loc.GetString(proto.Title);
        var sanitazed = $"[bold]\\[{str}][/bold]";
        if (proto.Color is not null)
            sanitazed = $"[bold]\\[[color={proto.Color}]{str}[/color]][/bold] ";

        return sanitazed;
    }
}
