using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;

namespace Content.Client._Gabystation.CartridgeLoader.Cartridges;

public sealed partial class NanoBankUi : UIFragment
{
    private NanoBankUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new NanoBankUiFragment();

        /*_fragment.OnNextButtonPressed += () =>
        {
            SendNewsReaderMessage(NewsReaderUiAction.Next, userInterface);
        };
        _fragment.OnPrevButtonPressed += () =>
        {
            SendNewsReaderMessage(NewsReaderUiAction.Prev, userInterface);
        };
        _fragment.OnNotificationSwithPressed += () =>
        {
            MessengerMessage(NewsReaderUiAction.NotificationSwitch, userInterface);
        };*/
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        /*switch (state)
        {
            case MessengerBoundUserInterfaceState cast:
                _fragment?.UpdateState(cast.Article, cast.TargetNum, cast.TotalNum, cast.NotificationOn);
                break;
            case MessengerEmptyBoundUserInterfaceState empty:
                _fragment?.UpdateEmptyState(empty.NotificationOn);
                break;
        }*/
    }

    /*private void SendNewsReaderMessage(NewsReaderUiAction action, BoundUserInterface userInterface)
    {
        var newsMessage = new NewsReaderUiMessageEvent(action);
        var message = new CartridgeUiMessage(newsMessage);
        userInterface.SendMessage(message);
    }*/
}
