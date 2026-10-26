using Content.Shared._CMU14.Xenomorphs;
using Content.Client._CMU14.Xenomorphs;
using Content.Client._Dumont.Announce;
using Content.Shared._Dumont.Announce;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._Dumont.Xenomorphs;

public sealed partial class XenomorphQueenAnnouncementSystem : EntitySystem
{
    [Dependency] private IUserInterfaceManager _ui = default!;
    [Dependency] private IPlayerManager _players = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;

    private AnnouncementWidget? _announcement;
    private EntityUid? _recipient;

    public override void Initialize()
    {
        SubscribeNetworkEvent<XenomorphQueenAnnouncementEvent>(OnAnnouncement);
    }

    private void OnAnnouncement(XenomorphQueenAnnouncementEvent args)
    {
        ClearAnnouncement();
        if (!HasComp<XenomorphComponent>(_players.LocalEntity) || _ui.ActiveScreen is not { } screen)
            return;

        var preset = _prototypes.Index<AnnouncementPresetPrototype>("XenoQueen");
        _recipient = _players.LocalEntity;
        _announcement = screen.GetOrAddWidget<AnnouncementWidget>();
        _announcement.PositioningReference = FindViewport(screen);
        _announcement.ShowAnnouncement(new AnnouncementNetData
        {
            Text = args.Message.Split('\n'),
            Style = preset.Style,
            Title = Loc.GetString("xenomorph-queen-announcement-title"),
            ShowSprite = preset.ShowSprite,
        });
    }

    public override void FrameUpdate(float frameTime)
    {
        if (_announcement != null && (_players.LocalEntity != _recipient || _announcement.Parent != _ui.ActiveScreen))
            ClearAnnouncement();
    }

    private static Control? FindViewport(Control parent)
    {
        if (parent.Name == "MainViewport") return parent;
        foreach (var child in parent.Children)
            if (FindViewport(child) is { } viewport) return viewport;
        return null;
    }

    private void ClearAnnouncement()
    {
        if (_announcement?.Parent is UIScreen screen)
            screen.RemoveWidget<AnnouncementWidget>();
        _announcement?.Dispose();
        _announcement = null;
        _recipient = null;
    }

    public override void Shutdown()
    {
        ClearAnnouncement();
    }
}
