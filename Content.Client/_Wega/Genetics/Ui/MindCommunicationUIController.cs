using Content.Shared.Genetics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Timing;

namespace Content.Client._Wega.Genetics.Ui;

public sealed class MindCommunicationUIController : UIController
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private MindCommunicationPanel? _panel;
    private bool _panelDisposed = false;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<MindCommunicationMenuOpenedEvent>(OnMenuReceived);
    }

    private void OnMenuReceived(MindCommunicationMenuOpenedEvent args, EntitySessionEventArgs eventArgs)
    {
        var session = IoCManager.Resolve<IPlayerManager>().LocalSession;
        var userEntity = _entityManager.GetEntity(args.Uid);

        if (session?.AttachedEntity.HasValue == true && session.AttachedEntity.Value == userEntity)
        {
            ShowPanel();
        }
    }

    public void ShowPanel()
    {
        if (_panel is null)
        {
            _panel = _uiManager.CreateWindow<MindCommunicationPanel>();
            _panel.OnClose += OnMenuClosed;
            _panel.OpenCentered();
        }
        else
        {
            _panel.OpenCentered();
        }

        Timer.Spawn(30000, () =>
        {
            if (_panel != null && !_panelDisposed)
                _panel.Close();
        });
    }

    private void OnMenuClosed()
    {
        _panelDisposed = true;
        _panel = null;
    }
}
