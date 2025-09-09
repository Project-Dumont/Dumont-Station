using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;
using Content.Shared._Gabystation.CartridgeLoader.Cartridges;

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

        _fragment.OnMessageSent += (type, targetAcc, content) =>
        {
            SendNanoBankUiMessage(type, targetAcc, content, userInterface);
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is NanoBankUiState cast)
            _fragment?.UpdateState(cast);
    }

    private static void SendNanoBankUiMessage(NanoBankUiMessageType type,
        int? targetAcc,
        float? content,
        BoundUserInterface userInterface)
    {
        var nanoChatMessage = new NanoBankUiMessageEvent(type, targetAcc, content);
        var message = new CartridgeUiMessage(nanoChatMessage);
        userInterface.SendMessage(message);
    }
}
